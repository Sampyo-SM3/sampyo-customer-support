package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.CodeDTO;
import com.example.connectBoard.repository.spc.CodeRepository;

@Service
public class CodeService {
    
    private final CodeRepository codeRepository;
    
    public CodeService(CodeRepository codeRepository) {
        this.codeRepository = codeRepository;
    }    

    public List<CodeDTO> getCodes(String category) {
    	 return codeRepository.getCodes(category);
    }
     
    public List<CodeDTO> getCodeCount(String startDate, String endDate, String writerId, String dpId) {
        return codeRepository.getCodeCount(startDate, endDate, writerId, dpId);
    }
    
}
