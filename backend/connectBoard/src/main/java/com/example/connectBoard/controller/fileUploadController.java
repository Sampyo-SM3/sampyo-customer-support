package com.example.connectBoard.controller;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.multipart.MultipartFile;

import com.example.connectBoard.config.PasswordManager;
import com.example.connectBoard.dto.CommentDTO;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.core.io.Resource;
import org.springframework.core.io.UrlResource;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;

import java.net.URLEncoder;


@Controller
@RequestMapping("/api")
public class fileUploadController {

    private Logger logger = LoggerFactory.getLogger(fileUploadController.class);
    
    String fileServerPassword = PasswordManager.getPassword("파일서버비밀번호");

    // 경로 !! ->  \\sp_file.sampyo.co.kr\idrive\SP_ERP_FILE\spc_file\WEB
    //private final String uploadLogisticsDir = "\\\\sp_file.sampyo.co.kr\\idrive\\SP_ERP_FILE\\spc_file\\WEB\\";

    String uploadDir = "\\\\sp_file.sampyo.co.kr\\idrive\\SP_ERP_FILE\\spc_file\\WEB\\csr\\";
    String targetDir = "\\\\sp_file.sampyo.co.kr\\idrive\\SP_ERP_FILE\\spc_file\\WEB\\csr";
    String g_id = "staerpfiles1";
    String g_pwd = fileServerPassword;

    @PostMapping("/fileUpload")
    public ResponseEntity<Map<String, Object>> handleFileUpload(@RequestParam("files") MultipartFile[] files) {
        Map<String, Object> response = new HashMap<>();
        List<Map<String, String>> fileList = new ArrayList<>();     
        
        try {
            //cmd 추가
            String netUseCommand = "cmd.exe /c net use " + targetDir + " /user:" + g_id + " " + g_pwd;
            
            try {
                Process process = Runtime.getRuntime().exec(netUseCommand);
                int exitCode = process.waitFor();  // waitFor() 호출
                if (exitCode != 0) {
                    logger.error("Command failed with exit code: " + exitCode);
                }
            } catch (IOException | InterruptedException e) {
                logger.error("Error occurred while executing the command", e);
                response.put("result", "failure");
                response.put("message", "파일 업로드 실패");
                return ResponseEntity.status(500).body(response);
            }
            
            for (MultipartFile file : files) {
                String originFile = file.getOriginalFilename();
                String changeFile = null;
                
                String fileSize = Long.toString(file.getSize() / 1024) + "KB";

                if (originFile.contains(".")) {
                    String ext = originFile.substring(originFile.lastIndexOf("."));
                    changeFile = UUID.randomUUID().toString() + ext;

                    Map<String, String> map = new HashMap<>();
                    map.put("originFile", originFile);
                    map.put("changeFile", changeFile);
                    map.put("fileSize", fileSize);
                    fileList.add(map);

                    logger.info("Origin File: " + originFile);
                    logger.info("Changed File: " + changeFile);
                }

                file.transferTo(new File(uploadDir + originFile));
            }

            response.put("result", "success");
            response.put("message", "파일 업로드 성공");
            response.put("files", fileList);  // 파일 정보를 추가
            return ResponseEntity.ok(response);

        } catch (IOException e) {
            logger.error("File upload failed", e);
            response.put("result", "failure");
            response.put("message", "파일 업로드 실패");
            return ResponseEntity.status(500).body(response);
        }
    }

    // 파일 삭제 메서드
    @PostMapping("/fileDelete")
    public ResponseEntity<Map<String, String>> handleFileDelete(@RequestParam("originFile") String originFile) {
        Map<String, String> response = new HashMap<>();
        
        try {
            String netUseCommand = "cmd.exe /c net use " + targetDir + " /user:" + g_id + " " + g_pwd;
            
            try {
                Process process = Runtime.getRuntime().exec(netUseCommand);
                int exitCode = process.waitFor();  // waitFor() 호출
                if (exitCode != 0) {
                    logger.error("Command failed with exit code: " + exitCode);
                }
            } catch (IOException | InterruptedException e) {
                logger.error("Error occurred while executing the command", e);
                response.put("result", "failure");
                response.put("message", "파일 업로드 실패");
                return ResponseEntity.status(500).body(response);
            }
            
            Path fileToDeletePath = Paths.get(uploadDir + originFile);
            Files.delete(fileToDeletePath);
            response.put("result", "success");
            response.put("message", "파일 삭제 성공");
            return ResponseEntity.ok(response);
        } catch (IOException e) {
            logger.error("File delete failed", e);
            response.put("result", "failure");
            response.put("message", "파일 삭제 실패");
            return ResponseEntity.status(500).body(response);
        }
    }

    @GetMapping("/download")
    public ResponseEntity<Resource> downloadFile(@RequestParam("filename") String filename) {
        String netUseCommand = "cmd.exe /c net use " + targetDir + " /user:" + g_id + " " + g_pwd;
        
        try {
            // 파일 경로 설정 (업로드된 디렉토리)
            Path filePath = Paths.get(uploadDir + filename).normalize();
            Resource resource = new UrlResource(filePath.toUri());

            if (!resource.exists()) {
                throw new FileNotFoundException("File not found: " + filename);
            }

            String encodedFilename = new String(resource.getFilename().getBytes("UTF-8"), "ISO-8859-1");
            encodedFilename = URLEncoder.encode(filename, "UTF-8").replaceAll("\\+", "%20");
            
            String contentDisposition = "attachment; filename=\"" + encodedFilename + "\"; filename*=UTF-8''" + encodedFilename;
            
            return ResponseEntity.ok()
                    .header(HttpHeaders.CONTENT_DISPOSITION, contentDisposition)
                    .body(resource);

        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
    }

}