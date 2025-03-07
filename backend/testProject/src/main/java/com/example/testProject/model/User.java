package com.example.testProject.model;

import java.util.Date;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class User {
    private String name;
    private int age;
    private String email;
    private Date enter_dt;
}
