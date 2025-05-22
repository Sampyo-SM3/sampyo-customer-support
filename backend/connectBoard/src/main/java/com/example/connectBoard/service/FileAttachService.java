package com.example.connectBoard.service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.FileAttachDTO;
import com.example.connectBoard.repository.spc.FileAttachRepository;

@Service
public class FileAttachService {
    
    private final FileAttachRepository fileattachRepository;
    
    public FileAttachService(FileAttachRepository fileattachMapper) {
        this.fileattachRepository = fileattachMapper;
    }    

    public void insertFileAttach(FileAttachDTO fileattach) {        
    	fileattachRepository.insertFileAttach(fileattach);
    }
    
    public void deleteFileAttach(FileAttachDTO fileattach) {        
    	fileattachRepository.deleteFileAttach(fileattach);
    }
    
    public List<FileAttachDTO> getFileList(Long seq, String boardType) {
        Map<String, Object> param = new HashMap<>();
        param.put("seq", seq);
        param.put("boardType", boardType);
        return fileattachRepository.getFileList(param);
    }
    
}
