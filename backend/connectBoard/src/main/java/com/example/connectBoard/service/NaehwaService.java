package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.NaeHwaDTO;
import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.repository.naehwa.NaeHwaRepository;

@Service
public class NaehwaService {
	private final NaeHwaRepository naeHwaRepository;
	
	public NaehwaService(NaeHwaRepository naeHwaRepository) {
		this.naeHwaRepository = naeHwaRepository;
	}
	
	
	public List<NaeHwaDTO> selectNaehwa(NaeHwaDTO naeHwaDTO) {
	    List<NaeHwaDTO> result = naeHwaRepository.selectNaehwaChartData(naeHwaDTO);
	    System.out.println(result);
	    return result;
	}
	
	public List<NaeHwaDTO> selectNaehwaConfig(NaeHwaDTO naeHwaDTO) {
	    List<NaeHwaDTO> result = naeHwaRepository.selectNaehwaConfig(naeHwaDTO);
	    System.out.println(result);
	    return result;
	}	
	
    public Integer selectNaehwaLength(NaeHwaDTO naeHwaDTO) { // 반환 타입을 Integer로 변경
        Integer result = naeHwaRepository.selectNaehwaLength(naeHwaDTO);
        System.out.println("Length result: " + result);
        return result;
    }
	
		

}
