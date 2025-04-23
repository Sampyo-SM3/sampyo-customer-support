package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.UserAuthDTO;
import com.example.connectBoard.repository.spc.UserAuthRepository;

@Service
public class UserAuthService {

	private final UserAuthRepository userAuthRepository;
	
    public UserAuthService(UserAuthRepository userAuthRepository) {
        this.userAuthRepository = userAuthRepository;
    }    

    public List<UserAuthDTO> getAllUserAuth() {    	
        return userAuthRepository.getAllUserAuth();
    }
    
    public List<UserAuthDTO> getUserAuth(String userId) {    	
        return userAuthRepository.getUserAuth(userId);
    }

}