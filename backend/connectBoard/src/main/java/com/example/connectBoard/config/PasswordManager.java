package com.example.connectBoard.config;

import java.io.StringReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Properties;

public class PasswordManager {
    private static Properties credentials = new Properties();
    
    static {
        try {
            // 상대 경로 또는 절대 경로 설정
            String content = Files.readString(Paths.get("src/main/java/com/example/connectBoard/config/password.txt"));
            credentials.load(new StringReader(content));
        } catch (Exception e) {
            System.err.println("비밀번호 파일을 읽을 수 없습니다: " + e.getMessage());
        }
    }
    
    public static String getPassword(String key) {
        return credentials.getProperty(key);
    }
}