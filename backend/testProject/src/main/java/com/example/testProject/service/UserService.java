package com.example.testProject.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.testProject.dto.UserDTO;
import com.example.testProject.repository.UserMapper;

@Service
public class UserService {

    private final UserMapper userMapper;

    public UserService(UserMapper userMapper) {
        this.userMapper = userMapper;
    }

    public List<UserDTO> getAllUsers() {
        return userMapper.getAllUsers();
    }
    
    public UserDTO getUser(String id) {
        return userMapper.getUser(id);
    }    
    
    public int updateUser(UserDTO user) {
        return userMapper.updateUser(user);
    }        
        
}
