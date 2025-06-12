package com.example.connectBoard.dto;

import java.time.LocalDate;
import java.time.LocalDateTime;

import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

@Getter
@Setter
@ToString
public class NaeHwaDTO {
    private String reportNo;
    private LocalDateTime repairFrDt;
    private LocalDateTime repairToDt;
    private Integer repairDay;
    private Float repairMeter;
    private String brickTypeCd;
    private Float repairMeterSt;
    private Float repairMeterCl;
    private String brickColor;
    
    private String wcCd;
    private String wcNm;
    private Integer wcMeter;
    private String wcZoneCd;
    private String wcZoneNm;
    private Float wcZoneSt;
    private Float wcZoneCl;
    private String wcZoneColor; 
    
     
    
    
    
    
    
    
   
    
    
}

