package com.example.connectBoard.repository.spc;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.LibraryDTO;

import java.util.List;

@Mapper
public interface LibraryRepository {
	
	List<LibraryDTO> searchLibrary(String title);
	
	LibraryDTO getLibrary(int seq);
	
	void insertLibrary(LibraryDTO library);
	
	void updateLibrary(LibraryDTO library);
	
	void deleteLibrary(LibraryDTO library);
	
	void addCnt();
	
}