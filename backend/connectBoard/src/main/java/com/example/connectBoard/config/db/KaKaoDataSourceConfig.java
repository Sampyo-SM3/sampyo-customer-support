package com.example.connectBoard.config.db;

import javax.sql.DataSource;

import org.apache.ibatis.session.SqlSessionFactory;
import org.mybatis.spring.SqlSessionFactoryBean;
import org.mybatis.spring.SqlSessionTemplate;
import org.mybatis.spring.annotation.MapperScan;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.boot.jdbc.DataSourceBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.io.support.PathMatchingResourcePatternResolver;
import org.springframework.jdbc.datasource.DataSourceTransactionManager;

@Configuration
@MapperScan(basePackages = {"com.example.connectBoard.repository.kakao"}, 
            sqlSessionFactoryRef = "kakaoSqlSessionFactory")
public class KaKaoDataSourceConfig {

    @Bean(name = "kakaoDataSource")
    @ConfigurationProperties(prefix = "spring.datasource.kakao")
    public DataSource blueaccountDataSource() {
        return DataSourceBuilder.create().build();
    }

    @Bean(name = "kakaoSqlSessionFactory")
    public SqlSessionFactory blueaccountSqlSessionFactory(@Qualifier("kakaoDataSource") DataSource dataSource) throws Exception {
        SqlSessionFactoryBean sqlSessionFactoryBean = new SqlSessionFactoryBean();
        sqlSessionFactoryBean.setDataSource(dataSource);
        sqlSessionFactoryBean.setMapperLocations(
                new PathMatchingResourcePatternResolver().getResources("classpath:mapper/kakao/**/*.xml"));
        
        org.apache.ibatis.session.Configuration configuration = new org.apache.ibatis.session.Configuration();
        configuration.setMapUnderscoreToCamelCase(true);
        sqlSessionFactoryBean.setConfiguration(configuration);
        
        return sqlSessionFactoryBean.getObject();
    }

    @Bean(name = "kakaoSqlSessionTemplate")
    public SqlSessionTemplate blueaccountSqlSessionTemplate(@Qualifier("kakaoSqlSessionFactory") SqlSessionFactory sqlSessionFactory) {
        return new SqlSessionTemplate(sqlSessionFactory);
    }

    @Bean(name = "kakaoTransactionManager")
    public DataSourceTransactionManager blueaccountTransactionManager(@Qualifier("kakaoDataSource") DataSource dataSource) {
        return new DataSourceTransactionManager(dataSource);
    }
}