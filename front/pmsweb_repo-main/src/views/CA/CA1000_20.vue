<template>
    <v-container fluid class="pa-10">
      <v-row no-gutters class="search-row">
        <!-- 요청기간 -->
        <v-col class="search-col request-period">
          <div class="label-box">요청기간</div>
          <div class="input-container pt-2 pb-2">
            <div class="date-wrapper">
              <v-text-field
                v-model="startDate"
                class="date-input"
                density="compact"
                hide-details
                readonly
                variant="outlined"
              ></v-text-field>
              <div class="calendar-icon-container">
                <v-btn icon class="calendar-btn">
                  <v-icon size="small" color="#2196F3">mdi-calendar</v-icon>
                </v-btn>
              </div>
            </div>
            
            <span class="date-separator">~</span>
            
            <div class="date-wrapper">
              <v-text-field
                v-model="endDate"
                class="date-input"
                density="compact"
                hide-details
                readonly
                variant="outlined"
              ></v-text-field>
              <div class="calendar-icon-container">
                <v-btn icon class="calendar-btn">
                  <v-icon size="small" color="#2196F3">mdi-calendar</v-icon>
                </v-btn>
              </div>
            </div>
            
            <div class="date-buttons">
              <div class="date-btn-container">
                <v-btn value="today" class="date-btn">오늘</v-btn>
                <v-btn value="week" class="date-btn">1주일</v-btn>
                <v-btn value="15days" class="date-btn">15일</v-btn>
                <v-btn value="month" class="date-btn">1개월</v-btn>
                <v-btn value="3months" class="date-btn">3개월</v-btn>
              </div>
            </div>
          </div>
        </v-col>
      </v-row>
    </v-container>
  </template>
  
  <script>
  export default {
    data() {
      return {
        startDate: '2025-03-03',
        endDate: '2025-03-10',
        dateRange: 'month'
      }
    }
  }
  </script>
  
  <style scoped>
  .search-row {
    display: flex;
    align-items: stretch;
    min-height: 40px;
    border-top: 1px solid #e0e0e0;
    border-bottom: 1px solid #e0e0e0;
  }
  
  .search-col {
    display: flex;
    align-items: center;
    padding: 0;
    border-right: 1px solid #e0e0e0;
  }
  
  .request-period {
    max-width: 550px;
    flex-grow: 0;
  }
  
  .label-box {
    width: 60px;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: 500;
    color: #333;
    background-color: #f0f0f0;
    white-space: nowrap;
    padding: 0 4px;
  }
  
  .input-container {
    display: flex;
    align-items: center;
    flex: 1;
    padding: 0 8px;
  }
  
  .date-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    width: 160px;
  }
  
  .date-input {
    width: 120px;
  }
  
  .date-input :deep(.v-field__input) {
    font-size: 15px;
  }
  
  .calendar-icon-container {
    display: flex;
    align-items: center;
    margin-left: 12px;
  }
  
  .calendar-btn {
    width: 28px;
    height: 28px;
    min-width: 28px;
    z-index: 1;
  }
  
  .date-separator {
    margin: 0 10px;
    font-size: 16px;
    color: #757575;
  }
  
  .date-buttons {
    margin-left: 20px;
  }
  
  .date-btn-container {
    display: flex;
  }
  
  .date-btn {
    min-width: 48px;
    padding: 0 12px;
    height: 32px;
    letter-spacing: -0.5px;
    border: 1px solid #dcdcdc;
    border-radius: 0;
    background-color: #ffffff;
    color: #333333;
    box-shadow: none;
    margin: 0;
  }
  
  .date-btn:not(:first-child) {
    border-left: none;
  }
  
  .date-btn:hover {
    background-color: #f5f5f5;
  }
  
  .date-btn.active {
    background-color: #e0f2fe;
    color: #2196F3;
  }
  </style>