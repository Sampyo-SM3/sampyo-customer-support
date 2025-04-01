package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.UserPopDTO;
import com.example.connectBoard.repository.spc.UserPopRepository;

@Service
public class UserPopService {

	private final UserPopRepository userPopRepository;
	
    public UserPopService(UserPopRepository userPopRepository) {
        this.userPopRepository = userPopRepository;
    }    

    public List<UserPopDTO> getAllUser(UserPopDTO userPop) {    	
        return userPopRepository.getAllUser(userPop);
    }  
}