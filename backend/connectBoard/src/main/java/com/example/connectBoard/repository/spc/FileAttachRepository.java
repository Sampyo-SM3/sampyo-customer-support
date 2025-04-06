package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.FileAttachDTO;

@Mapper
public interface FileAttachRepository { 
    void insertFileAttach(FileAttachDTO fileattach);   
    
    void deleteFileAttach(FileAttachDTO fileattach);    
    
    List<FileAttachDTO> getFileList(Long seq);

}