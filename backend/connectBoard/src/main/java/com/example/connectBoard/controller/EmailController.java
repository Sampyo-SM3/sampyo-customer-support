package com.example.connectBoard.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;

import jakarta.mail.internet.MimeMessage;

@RestController
@RequestMapping("/api/email")
public class EmailController {

    private final JavaMailSender mailSender;
    
 
    public EmailController(JavaMailSender mailSender) {
        this.mailSender = mailSender;
    }
    
    @PostMapping("/send")
    public ResponseEntity<String> sendEmail(    		
        @RequestParam("to") String to,
        @RequestParam("subject") String subject,
        @RequestParam("message") String message
        // @RequestParam(value = "attachments", required = false) MultipartFile[] attachments
    ) {        
        try {
            MimeMessage mimeMessage = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(mimeMessage, true);
            
            helper.setTo(to);
            helper.setSubject(subject);
            helper.setText(message, true); // HTML 지원
            
//            // 첨부 파일 처리
//            if (attachments != null) {
//                for (MultipartFile file : attachments) {
//                    helper.addAttachment(file.getOriginalFilename(), file);
//                }
//            }

            mailSender.send(mimeMessage);
            return ResponseEntity.ok("이메일이 성공적으로 전송되었습니다.");
        } catch (Exception e) {
        	System.out.println("이메일 전송 실패: " + e.getMessage());
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("이메일 전송 실패: " + e.getMessage());
        }
    }
}