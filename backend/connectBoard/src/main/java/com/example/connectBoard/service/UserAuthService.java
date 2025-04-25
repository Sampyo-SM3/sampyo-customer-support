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

    /**
     * 메뉴 권한 데이터 upsert (존재 시 update, 없으면 insert)
     */
    public void saveOrUpdateUserAuth(UserAuthDTO userAuthDTO) {
        int count = userAuthRepository.checkMenuAuthExists(userAuthDTO);
        System.out.println("🧮 checkMenuAuthExists count = " + count);
        
        if (count > 0) {
            userAuthRepository.updateMenuAuth(userAuthDTO);
        } else {
            userAuthRepository.insertMenuAuth(userAuthDTO);
        }
    }
    
    public void deleteUserAuth(UserAuthDTO userAuthDTO) {    	
       userAuthRepository.deleteUserAuth(userAuthDTO);
    }
  
}