package com.example.connectBoard.repository.spc;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.FileAttachDTO;

@Mapper
public interface FileAttachRepository { 
    void insertFileAttach(FileAttachDTO fileattach);    
}