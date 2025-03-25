package com.example.connectBoard.service;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.EmployeePreferenceDto;
import com.example.connectBoard.exception.Exceptions.PasswordMismatchException;
import com.example.connectBoard.exception.Exceptions.UserNotFoundException;
import com.example.connectBoard.repository.spc.LoginRepository;
import com.example.connectBoard.repository.blueaccount.BlueLoginRepository;

@Service
public class LoginService {

    private final LoginRepository loginRepository;
    private final BlueLoginRepository blueLoginRepository; // BlueAccount 레포지토리 추가

    // 생성자에 BlueLoginRepository 주입 추가
    public LoginService(LoginRepository loginRepository, BlueLoginRepository blueLoginRepository) {
        this.loginRepository = loginRepository;
        this.blueLoginRepository = blueLoginRepository;
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
    
    public EmployeePreferenceDto validate_blue_id(String id, String password) {    	
        // 1. 사용자 ID 존재 여부 확인
        EmployeePreferenceDto employee = blueLoginRepository.findBlueAccountById(id);        

        if (employee == null) {
            throw new UserNotFoundException("User not found with id: " + id);
        }
        System.out.println("2");



        // 3. 로그인 성공 - 사용자 정보 반환
        return employee;
    }    


}