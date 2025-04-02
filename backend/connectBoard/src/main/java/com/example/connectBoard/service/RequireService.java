package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.repository.spc.RequireRepository;

@Service
public class RequireService {

    private final RequireRepository requireRepository;
    
    public RequireService(RequireRepository requireMapper) {
        this.requireRepository = requireMapper;
    }

    public List<RequireDTO> getAllRequires() {
        return requireRepository.getAllRequires();
    }
    
    public RequireDTO getRequire(int seq) {
        return requireRepository.getRequire(seq);
    }
    
    public List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria) {
        // 매퍼를 통해 검색 조건으로 데이터 조회
        return requireRepository.searchRequiresByCriteria(criteria);
    }   
    
    public int insertRequire(RequireDTO require) {
    	requireRepository.insertRequire(require);    	
                
        // MyBatis에서 자동 생성된 seq 반환
        return require.getSeq();    	
    }   

    public void updateForm(RequireDTO require) {
    	requireRepository.updateForm(require);    	
    }  
    
    public void updateSrForm(RequireDTO require) {
    	requireRepository.updateSrForm(require);    	
    }   

}
