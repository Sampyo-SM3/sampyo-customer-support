package com.example.testProject.repository;

import com.example.testProject.dto.UserDTO;
import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Select;

import java.util.List;

@Mapper
public interface UserMapper {
/*    @Select("SELECT * FROM User")*/
    List<UserDTO> getAllUsers();
    
    UserDTO getUser(String id);
    
    int updateUser(UserDTO user);
}