package com.example.testProject.service;

import org.springframework.stereotype.Service;

@Service
public class testService {

    public String getHelloMessage() {
        return "Hello, 조희자이 대리님";
    }

    public String getGoodbyeMessage() {
        return "Goodbye, 조희자이대리님!";
    }
}
