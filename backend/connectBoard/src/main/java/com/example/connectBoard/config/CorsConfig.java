package com.example.connectBoard.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.CorsRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

@Configuration
public class CorsConfig {
    @Bean
    public WebMvcConfigurer corsConfigurer() {
        return new WebMvcConfigurer() {
            @Override
            public void addCorsMappings(CorsRegistry registry) {
                registry.addMapping("/**")
                .allowedOrigins(
                        "http://localhost:8081", 
                        "http://localhost:8080", 
                        "http://192.168.41.155:8081",
                        "http://localhost:29000",
                        "http://172.17.22.120:8080",
                        "http://172.17.22.120:8081",
                        "http://172.17.22.120:8082",
                        "http://172.17.22.120:8083",
                        "http://172.17.22.120:8084",
                        "http://172.17.22.118:8081"
                    )
                        .allowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .allowedHeaders("*")
                        .allowCredentials(true);
            }
        };
    }
}
