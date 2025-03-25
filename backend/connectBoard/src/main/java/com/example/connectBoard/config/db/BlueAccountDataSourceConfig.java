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
@MapperScan(basePackages = {"com.example.connectBoard.repository.blueaccount"}, 
            sqlSessionFactoryRef = "blueaccountSqlSessionFactory")
public class BlueAccountDataSourceConfig {

    @Bean(name = "blueaccountDataSource")
    @ConfigurationProperties(prefix = "spring.datasource.blueaccount")
    public DataSource blueaccountDataSource() {
        return DataSourceBuilder.create().build();
    }

    @Bean(name = "blueaccountSqlSessionFactory")
    public SqlSessionFactory blueaccountSqlSessionFactory(@Qualifier("blueaccountDataSource") DataSource dataSource) throws Exception {
        SqlSessionFactoryBean sqlSessionFactoryBean = new SqlSessionFactoryBean();
        sqlSessionFactoryBean.setDataSource(dataSource);
        sqlSessionFactoryBean.setMapperLocations(
                new PathMatchingResourcePatternResolver().getResources("classpath:mapper/blueaccount/**/*.xml"));
        
        org.apache.ibatis.session.Configuration configuration = new org.apache.ibatis.session.Configuration();
        configuration.setMapUnderscoreToCamelCase(true);
        sqlSessionFactoryBean.setConfiguration(configuration);
        
        return sqlSessionFactoryBean.getObject();
    }

    @Bean(name = "blueaccountSqlSessionTemplate")
    public SqlSessionTemplate blueaccountSqlSessionTemplate(@Qualifier("blueaccountSqlSessionFactory") SqlSessionFactory sqlSessionFactory) {
        return new SqlSessionTemplate(sqlSessionFactory);
    }

    @Bean(name = "blueaccountTransactionManager")
    public DataSourceTransactionManager blueaccountTransactionManager(@Qualifier("blueaccountDataSource") DataSource dataSource) {
        return new DataSourceTransactionManager(dataSource);
    }
}