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
@MapperScan(basePackages = {"com.example.connectBoard.repository.naehwa"}, 
            sqlSessionFactoryRef = "naehwaSqlSessionFactory")
public class NaeHwaDataSourceConfig {

    @Bean(name = "naehwaDataSource")
    @ConfigurationProperties(prefix = "spring.datasource.naehwa")
    public DataSource NaehwaDataSource() {
        return DataSourceBuilder.create().build();
    }

    @Bean(name = "naehwaSqlSessionFactory")
    public SqlSessionFactory blueaccountSqlSessionFactory(@Qualifier("naehwaDataSource") DataSource dataSource) throws Exception {
        SqlSessionFactoryBean sqlSessionFactoryBean = new SqlSessionFactoryBean();
        sqlSessionFactoryBean.setDataSource(dataSource);
        sqlSessionFactoryBean.setMapperLocations(
                new PathMatchingResourcePatternResolver().getResources("classpath:mapper/naehwa/**/*.xml"));
        
        org.apache.ibatis.session.Configuration configuration = new org.apache.ibatis.session.Configuration();
        configuration.setMapUnderscoreToCamelCase(true);
        sqlSessionFactoryBean.setConfiguration(configuration);
        
        return sqlSessionFactoryBean.getObject();
    }

    @Bean(name = "naehwaSqlSessionTemplate")
    public SqlSessionTemplate naehwaSqlSessionTemplate(@Qualifier("naehwaSqlSessionFactory") SqlSessionFactory sqlSessionFactory) {
        return new SqlSessionTemplate(sqlSessionFactory);
    }

    @Bean(name = "naehwaTransactionManager")
    public DataSourceTransactionManager naehwaTransactionManager(@Qualifier("naehwaDataSource") DataSource dataSource) {
        return new DataSourceTransactionManager(dataSource);
    }
}