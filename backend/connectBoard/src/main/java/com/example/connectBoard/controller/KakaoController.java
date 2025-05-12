package com.example.connectBoard.controller;

import com.example.connectBoard.dto.KakaoDTO;
import com.example.connectBoard.service.KakaoService;

import java.util.Map;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api")
public class KakaoController {

	private final KakaoService kakaoService;

	// ✅ 두 개의 서비스 모두 초기화
	public KakaoController(KakaoService kakaoService) {
		this.kakaoService = kakaoService;
	}

	@PostMapping("/kakao")
	public ResponseEntity<?> insertKakao(@RequestBody Map<String, String> request) {
//		System.out.println("-- insertKakao --");
		try {
			String content = request.get("content");
			String phone = request.get("phone");
			String templateCode = request.get("templateCode");
//			System.out.println("!!!!!!!!!!");
//			System.out.println(phone);
			// 조희재테스트
//			kakaoService.insertKakao(content, phone, templateCode);
			return ResponseEntity.ok("카카오톡 전송 성공!");
		} catch (Exception e) {
			return ResponseEntity.status(500).body("카카오 전송 실패: " + e.getMessage());
		}
	}
}
