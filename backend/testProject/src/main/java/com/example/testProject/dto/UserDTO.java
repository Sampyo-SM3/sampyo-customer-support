package com.example.testProject.dto;

import java.time.LocalDate;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserDTO {
    private int id;
    private String name;
    private int age;
    private String email;
    private LocalDate enter_dt;  // ✅ LocalDate 유지

}