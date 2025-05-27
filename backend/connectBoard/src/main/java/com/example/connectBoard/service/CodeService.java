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
     
    public List<CodeDTO> getCodeCount(CodeDTO code) {
        return codeRepository.getCodeCount(code);
    }
    
    public List<CodeDTO> getCodeCountUser(CodeDTO code) {
        return codeRepository.getCodeCountUser(code);
    }
    
    public List<CodeDTO> getCodeCountDepart(CodeDTO code) {
        return codeRepository.getCodeCountDepart(code);
    }    

    
}
