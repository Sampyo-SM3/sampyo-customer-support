package com.example.connectBoard.service;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.EmployeePreferenceDto;
import com.example.connectBoard.exception.Exceptions.PasswordMismatchException;
import com.example.connectBoard.exception.Exceptions.UserNotFoundException;
import com.example.connectBoard.repository.LoginRepository;

@Service
public class LoginService {

    private final LoginRepository loginRepository;

    public LoginService(LoginRepository loginRepository) {
        this.loginRepository = loginRepository;
    }

    public EmployeePreferenceDto login(String id, String password, String companyCd) {
        // 1. 사용자 ID 존재 여부 확인
        EmployeePreferenceDto employee = loginRepository.findByIdAndCompanyCd(id, companyCd);
        
        if (employee == null) {
            throw new UserNotFoundException("User not found with id: " + id);
        }

        // 2. 비밀번호 일치 여부 확인
        boolean isPasswordMatch = loginRepository.verifyPassword(password, employee.getPwd());
        
        if (!isPasswordMatch) {
            throw new PasswordMismatchException("Password does not match for user: " + id);
        }

        // 비밀번호 정보는 응답에서 제외
        employee.setPwd(null);
        
        // 3. 로그인 성공 - 사용자 정보 반환
        return employee;
    }
}