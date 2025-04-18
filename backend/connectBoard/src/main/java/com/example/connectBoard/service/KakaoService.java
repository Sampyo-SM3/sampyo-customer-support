package com.example.connectBoard.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.KakaoDTO;
import com.example.connectBoard.repository.kakao.KakaoRepository;

@Service
public class KakaoService {

    @Autowired
    private KakaoRepository kakaoRepository;

    public void insertKakao(String content, String phone) {        
    	kakaoRepository.insertKakao(content, phone);
    }    
}