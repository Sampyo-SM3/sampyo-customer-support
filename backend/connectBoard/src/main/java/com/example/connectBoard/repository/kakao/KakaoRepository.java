package com.example.connectBoard.repository.kakao;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.connectBoard.dto.KakaoDTO;

@Mapper
public interface KakaoRepository {
    
	int insertKakao(@Param("content") String content, @Param("phone") String phone);

}