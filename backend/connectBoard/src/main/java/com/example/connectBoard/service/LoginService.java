package com.example.connectBoard.service;

import java.util.Map;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.dto.EmployeePreferenceDto;
import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.UserAuthDTO;
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

    public EmployeePreferenceDto login(String id, String password, String deptCd, String companyCd, String phone, String email) {
//        System.out.println("서비스 로그인");
//        System.out.println("ID: " + id);
//        System.out.println("companyCd: " + companyCd);
        
        try {
            // 1. 사용자 ID 존재 여부 확인
            EmployeePreferenceDto employee = loginRepository.findByIdAndCompanyCd(id, companyCd);
//            System.out.println("사용자 조회 성공: " + (employee != null));
            
            if (employee == null) {
                throw new UserNotFoundException("User not found with id: " + id);
            }
            
            // 2. 비밀번호 일치 여부 확인
            boolean isPasswordMatch = loginRepository.verifyPassword(password, employee.getPwd());
//            System.out.println("비밀번호 검증 결과: " + isPasswordMatch);
            
            if (!isPasswordMatch) {
                throw new PasswordMismatchException("Password does not match for user: " + id);
            }
            
            // 비밀번호 정보는 응답에서 제외
            employee.setPwd(null);
                                   
            // 3. 로그인 성공 - 사용자 정보 반환
            return employee;
        } catch (Exception e) {
//            System.err.println("로그인 처리 중 예외 발생: " + e.getClass().getName());
//            System.err.println("예외 메시지: " + e.getMessage());
            e.printStackTrace();
            throw e; // 원래 예외를 다시 던져서 상위 레벨에서 처리할 수 있게 함
        }
    }
    
    public EmployeePreferenceDto validate_blue_id(String id, String password) {
        // 1. 사용자 ID 존재 여부 확인
        EmployeePreferenceDto employee = blueLoginRepository.findBlueAccountById(id);        

        if (employee == null) {
            throw new UserNotFoundException("User not found with id: " + id);            
        }
        // 3. 로그인 성공 - 사용자 정보 반환
        return employee;
    }    
    
    
    public void insertUser(String id, String password, String companyCd, String deptCd, String name, String phone, String email) {        
        loginRepository.insertUser(companyCd, id, name, password, deptCd, phone, email);
        
    }
    
    public boolean chkExistUserId(String id) {
        return loginRepository.chkExistUserId(id);
    }
	
    public void resetPassword(UserAuthDTO userAuth) {
    	loginRepository.resetPassword(userAuth);    	
    }

 


}