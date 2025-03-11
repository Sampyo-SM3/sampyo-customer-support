package com.example.testProject.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.testProject.dto.RequireDTO;
import com.example.testProject.dto.RequireSearchCriteria;
import com.example.testProject.repository.RequireRepository;

@Service
public class RequireService {

    private final RequireRepository requireRepositoy;
    
    public RequireService(RequireRepository requireMapper) {
        this.requireRepositoy = requireMapper;
    }

    public List<RequireDTO> getAllRequires() {
        return requireRepositoy.getAllRequires();
    }
    
    public RequireDTO getRequire(int seq) {
        return requireRepositoy.getRequire(seq);
    }
    
    public List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria) {
        // 매퍼를 통해 검색 조건으로 데이터 조회
        return requireRepositoy.searchRequiresByCriteria(criteria);
    }    

}
