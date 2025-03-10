package com.example.testProject.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.testProject.dto.RequireDTO;
import com.example.testProject.repository.RequireMapper;

@Service
public class RequireService {

    private final RequireMapper requireMapper;
    
    public RequireService(RequireMapper requireMapper) {
        this.requireMapper = requireMapper;
    }

    public List<RequireDTO> getAllRequires() {
        return requireMapper.getAllRequires();
    }
    
    public RequireDTO getRequire(int seq) {
        return requireMapper.getRequire(seq);
    }    

}
