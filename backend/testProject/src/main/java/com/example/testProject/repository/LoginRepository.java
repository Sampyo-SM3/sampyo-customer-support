package com.example.testProject.repository;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.testProject.dto.EmployeePreferenceDto;

@Mapper
public interface LoginRepository {
    
    /**
     * 사용자 ID와 회사코드로 사용자 정보 조회
     * 
     * @param id 사용자 ID
     * @param companyCd 회사 코드
     * @return 사용자 정보 DTO, 없으면 null
     */
    EmployeePreferenceDto findByIdAndCompanyCd(
            @Param("id") String id, 
            @Param("companyCd") String companyCd);
    
    /**
     * 사용자 존재 여부 확인
     * 
     * @param id 사용자 ID
     * @param companyCd 회사 코드
     * @return 존재하면 true, 아니면 false
     */
    boolean isUserExists(
            @Param("id") String id, 
            @Param("companyCd") String companyCd);
    
    /**
     * 비밀번호 검증 - PWDCOMPARE 함수 사용
     * 
     * @param rawPassword 사용자가 입력한 원본 비밀번호
     * @param storedPassword DB에 저장된 암호화된 비밀번호
     * @return 일치하면 true, 아니면 false
     */
    boolean verifyPassword(
            @Param("rawPassword") String rawPassword, 
            @Param("storedPassword") byte[] storedPassword);
    
    /**
     * 다른 방식의 비밀번호 검증 - 암호화한 값 직접 비교
     * 
     * @param rawPassword 사용자가 입력한 원본 비밀번호
     * @param storedPassword DB에 저장된 암호화된 비밀번호
     * @return 일치하면 true, 아니면 false
     */
    boolean verifyPasswordAlternative(
            @Param("rawPassword") String rawPassword, 
            @Param("storedPassword") byte[] storedPassword);
    
    /**
     * 사용자 ID, 회사코드, 비밀번호로 일치 여부 확인 (단일 쿼리)
     * 
     * @param id 사용자 ID
     * @param companyCd 회사 코드
     * @param password 사용자가 입력한 원본 비밀번호
     * @return 모두 일치하면 true, 아니면 false
     */
    boolean checkPasswordMatch(
            @Param("id") String id, 
            @Param("companyCd") String companyCd,
            @Param("password") String password);
}