package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.StatusDTO;
import com.example.connectBoard.repository.spc.StatusRepository;

@Service
public class StatusService {
    
    private final StatusRepository statusRepository;
    
    public StatusService(StatusRepository statusRepository) {
        this.statusRepository = statusRepository;
    }    

    public void updateStatus(RequireDTO require) { 
//    	System.out.println("-- require --");
//    	System.out.println(require);
    	statusRepository.updateStatus(require);
    }
    
}
