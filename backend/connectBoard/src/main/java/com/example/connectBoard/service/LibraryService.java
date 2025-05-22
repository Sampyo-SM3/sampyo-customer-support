package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.LibraryDTO;
import com.example.connectBoard.repository.spc.LibraryRepository;

@Service
public class LibraryService {

    private final LibraryRepository libraryRepository;
    
    public LibraryService(LibraryRepository libraryRepository) {
        this.libraryRepository = libraryRepository;
    }
    

    public List<LibraryDTO> searchLibrary(String title) {
        // 매퍼를 통해 검색 조건으로 데이터 조회
    	
        return libraryRepository.searchLibrary(title);
    }   
    
    public LibraryDTO getLibrary(int seq) {
    	
        return libraryRepository.getLibrary(seq);
    }
    
    public int insertLibrary(LibraryDTO library) {
    	libraryRepository.insertLibrary(library);    	

        return library.getSeq();    	
    }   

    public void updateLibrary(LibraryDTO library) {
    	libraryRepository.updateLibrary(library);    	
    } 
    
    public void deleteLibrary(LibraryDTO library) {
    	libraryRepository.deleteLibrary(library);    	
    } 
    
    public void addCnt() {
    	libraryRepository.addCnt();    	
    } 
    
  

}
