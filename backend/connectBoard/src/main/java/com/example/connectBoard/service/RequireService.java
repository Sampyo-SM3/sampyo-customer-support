package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.repository.spc.RequireRepository;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.*;
import java.util.stream.Collectors;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

@Service
public class RequireService {

	private final RequireRepository requireRepository;

	public RequireService(RequireRepository requireMapper) {
		this.requireRepository = requireMapper;
	}

	public List<RequireDTO> getAllRequires() {
		return requireRepository.getAllRequires();
	}

	public RequireDTO getRequire(int seq) {
		return requireRepository.getRequire(seq);
	}

	public List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria) {
		// 매퍼를 통해 검색 조건으로 데이터 조회

		return requireRepository.searchRequiresByCriteria(criteria);
	}

	public List<RequireDTO> getDashboardData(RequireSearchCriteria criteria) {
		// 매퍼를 통해 검색 조건으로 데이터 조회
		return requireRepository.getDashboardData(criteria);
	}

	public List<Map<String, Object>> getDashBoardMonthlyTotal(RequireSearchCriteria criteria) {
//	    System.out.println("--getDashBoardMonthlyTotal--");
//	    System.out.println(criteria.toString());

	    // 매퍼를 통해 검색 조건으로 데이터 조회 (Map으로 받기)
	    List<Map<String, Object>> rawData = requireRepository.getDashBoardMonthlyTotal(criteria);
//	    System.out.println("DB 조회 결과: " + rawData.toString());
	    
	    // 시작일과 종료일 파싱
	    LocalDate startDate = parseDate(criteria.getStartDate());
	    LocalDate endDate = parseDate(criteria.getEndDate());

	    if (startDate == null || endDate == null) {
	        return rawData; // 날짜가 없으면 원본 데이터 반환
	    }

	    // 전체 기간의 모든 월 생성
	    List<Map<String, Object>> totalData = generateAllMonthsInRangeAsMap(startDate, endDate);

	    // 실제 데이터와 매칭하여 값 채우기
	    Map<String, Map<String, Object>> dataMap = rawData.stream()
	        .collect(Collectors.toMap(
	            item -> item.get("year_val") + "-" + String.format("%02d", item.get("month_val")),
	            item -> item
	        ));

	    // 완전한 데이터 리스트 생성
	    List<Map<String, Object>> result = totalData.stream()
	        .map(monthData -> {
	            String key = monthData.get("year_val") + "-" + String.format("%02d", monthData.get("month_val"));
	            Map<String, Object> actualData = dataMap.get(key);
	            
	            if (actualData != null) {
	                return actualData; // 실제 데이터가 있으면 사용
	            } else {
	                // 데이터가 없으면 0으로 설정
	                monthData.put("completed_count", 0);
	                return monthData;
	            }
	        })
	        .collect(Collectors.toList());

	    // 결과 출력
//	    System.out.println("=== 월별 데이터 조회 결과 ===");
//	    System.out.println("조회 기간: " + criteria.getStartDate() + " ~ " + criteria.getEndDate());
//	    System.out.println("총 데이터 건수: " + result.size());
//	    for (Map<String, Object> data : result) {
//	        System.out.println(String.format("%d년 %s: %d건", 
//	            data.get("year_val"), data.get("month_name"), data.get("completed_count")));
//	    }
//	    System.out.println("=============================");

	    return result;
	}

	/**
	 * 문자열 날짜를 LocalDate로 파싱
	 */
	private LocalDate parseDate(String dateStr) {
	    if (dateStr == null || dateStr.trim().isEmpty()) {
	        return null;
	    }
	    
	    try {
	        // 공백 제거
	        dateStr = dateStr.trim();
	        
	        // 시간 정보가 포함된 경우 (예: "2025-01-01 00:00:00")
	        if (dateStr.contains(" ")) {
	            String datePart = dateStr.split(" ")[0];
	            return LocalDate.parse(datePart);
	        }
	        // 날짜만 있는 경우 (예: "2025-01-01")
	        else if (dateStr.contains("-")) {
	            return LocalDate.parse(dateStr);
	        }
	        // yyyyMMdd 형식
	        else if (dateStr.length() == 8) {
	            return LocalDate.parse(dateStr, DateTimeFormatter.ofPattern("yyyyMMdd"));
	        }
	        // yyyyMMddHHmmss 형식
	        else if (dateStr.length() == 14) {
	            return LocalDate.parse(dateStr.substring(0, 8), DateTimeFormatter.ofPattern("yyyyMMdd"));
	        }
	        else {
	            return LocalDate.parse(dateStr);
	        }
	    } catch (Exception e) {
	        System.err.println("날짜 파싱 실패: " + dateStr + ", " + e.getMessage());
	        return null;
	    }
	}

	/**
	 * 시작일부터 종료일까지의 모든 월을 Map으로 생성
	 */
	private List<Map<String, Object>> generateAllMonthsInRangeAsMap(LocalDate startDate, LocalDate endDate) {
	    List<Map<String, Object>> allMonths = new ArrayList<>();
	    
	    // 시작 월의 첫 날로 설정
	    LocalDate current = startDate.withDayOfMonth(1);
	    // 종료 월의 첫 날로 설정
	    LocalDate end = endDate.withDayOfMonth(1);
	    
	    while (!current.isAfter(end)) {
	        Map<String, Object> monthData = new HashMap<>();
	        monthData.put("year_val", current.getYear());
	        monthData.put("month_val", current.getMonthValue());
	        monthData.put("month_name", getMonthName(current.getMonthValue()));
	        monthData.put("completed_count", 0); // 기본값 0
	        
	        allMonths.add(monthData);
	        
	        // 다음 달로 이동
	        current = current.plusMonths(1);
	    }
	    
//	    System.out.println("생성된 전체 월 데이터: " + allMonths.size() + "개월");
	    return allMonths;
	}

	/**
	 * 월 숫자를 한국어 월명으로 변환
	 */
	private String getMonthName(int month) {
	    String[] monthNames = {
	        "1월", "2월", "3월", "4월", "5월", "6월",
	        "7월", "8월", "9월", "10월", "11월", "12월"
	    };
	    return monthNames[month - 1];
	}

	public List<RequireDTO> searchRequiresByCriteriaUser(RequireSearchCriteria criteria) {
		// 매퍼를 통해 검색 조건으로 데이터 조회

		return requireRepository.searchRequiresByCriteriaUser(criteria);
	}

	public List<RequireDTO> searchRequiresByCriteriaDepart(RequireSearchCriteria criteria) {
		// 매퍼를 통해 검색 조건으로 데이터 조회

		return requireRepository.searchRequiresByCriteriaDepart(criteria);
	}

	public List<RequireDTO> searchRequiresByCriteriaDepartAdmin(RequireSearchCriteria criteria) {
		// 매퍼를 통해 검색 조건으로 데이터 조회

		return requireRepository.searchRequiresByCriteriaDepartAdmin(criteria);
	}

	public int insertRequire(RequireDTO require) {
		requireRepository.insertRequire(require);

		// MyBatis에서 자동 생성된 seq 반환
		return require.getSeq();
	}

	public void updateForm(RequireDTO require) {
		requireRepository.updateForm(require);
	}

	public void updateSrForm(RequireDTO require) {
		requireRepository.updateSrForm(require);
	}



}
