package com.example.connectBoard.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.StatusDTO;
import com.example.connectBoard.repository.StatusRepository;

@Service
public class StatusService {
    
    private final StatusRepository statusRepository;
    
    public StatusService(StatusRepository statusRepository) {
        this.statusRepository = statusRepository;
    }    

    public List<StatusDTO> getAllStatuses() {
    	 return statusRepository.getAllStatuses();
    }

    
}
