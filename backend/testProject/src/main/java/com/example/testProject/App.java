package com.example.testProject;

import org.mybatis.spring.annotation.MapperScan;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.WebApplicationType;

@SpringBootApplication
@MapperScan("com.example.testProject.repository")  // ✅ MyBatis 매퍼 자동 스캔
public class App {
    public static void main(String[] args) {
        SpringApplication app = new SpringApplication(App.class);
        app.setWebApplicationType(WebApplicationType.SERVLET); // 🚀 강제 설정
        app.run(args);
    }
}
