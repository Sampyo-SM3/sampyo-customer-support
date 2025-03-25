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
import org.springframework.context.annotation.Primary;
import org.springframework.core.io.support.PathMatchingResourcePatternResolver;
import org.springframework.jdbc.datasource.DataSourceTransactionManager;

@Configuration
@MapperScan(basePackages = {"com.example.connectBoard.repository.spc"}, 
            sqlSessionFactoryRef = "spcSqlSessionFactory")
public class SpcDataSourceConfig {

    @Primary
    @Bean(name = "spcDataSource")
    @ConfigurationProperties(prefix = "spring.datasource.spc")
    public DataSource spcDataSource() {
        return DataSourceBuilder.create().build();
    }

    @Primary
    @Bean(name = "spcSqlSessionFactory")
    public SqlSessionFactory spcSqlSessionFactory(@Qualifier("spcDataSource") DataSource dataSource) throws Exception {
        SqlSessionFactoryBean sqlSessionFactoryBean = new SqlSessionFactoryBean();
        sqlSessionFactoryBean.setDataSource(dataSource);
        sqlSessionFactoryBean.setMapperLocations(
                new PathMatchingResourcePatternResolver().getResources("classpath:mapper/spc/**/*.xml"));
        
        org.apache.ibatis.session.Configuration configuration = new org.apache.ibatis.session.Configuration();
        configuration.setMapUnderscoreToCamelCase(true);
        sqlSessionFactoryBean.setConfiguration(configuration);
        
        return sqlSessionFactoryBean.getObject();
    }

    @Primary
    @Bean(name = "spcSqlSessionTemplate")
    public SqlSessionTemplate spcSqlSessionTemplate(@Qualifier("spcSqlSessionFactory") SqlSessionFactory sqlSessionFactory) {
        return new SqlSessionTemplate(sqlSessionFactory);
    }

    @Primary
    @Bean(name = "spcTransactionManager")
    public DataSourceTransactionManager spcTransactionManager(@Qualifier("spcDataSource") DataSource dataSource) {
        return new DataSourceTransactionManager(dataSource);
    }
}