<template>
  <v-card elevation="0" class="search-container">
    <v-expand-transition>
      <div v-show="isExpanded" class="search-content">
        
        <!-- 테이블 형식 검색 필터 -->
        <v-table class="search-table" density="compact">
          <tbody>
            <!-- 첫 번째 행: 요청기간 + 버튼 (항상 고정) -->
            <tr class="date-row">
              <td class="label-cell">
                <span class="filter-label">요청기간</span>
              </td>
              <td class="input-cell date-cell" colspan="2">
                <div class="date-section">
                  <!-- 날짜 입력 영역 -->
                  <div class="date-input-section">
                    <div class="date-picker-wrapper">
                      <VueDatePicker 
                        v-model="localStartDate"
                        class="date-picker" 
                        :teleport="true" 
                        position="bottom" 
                        :enable-time-picker="false" 
                        auto-apply 
                        locale="ko" 
                        format="yyyy-MM-dd"
                        :clearable="false" 
                        :text-input="false" 
                        @update:model-value="onStartDateChange"
                      />
                    </div>
                    
                    <span class="date-separator">~</span>
                    
                    <div class="date-picker-wrapper">
                      <VueDatePicker 
                        v-model="localEndDate"
                        class="date-picker" 
                        :teleport="true" 
                        position="bottom" 
                        :enable-time-picker="false" 
                        auto-apply 
                        locale="ko" 
                        format="yyyy-MM-dd"
                        :clearable="false"
                        :text-input="false" 
                        @update:model-value="onEndDateChange"
                      />
                    </div>
                  </div>
                  
                  <!-- 빠른 선택 버튼 -->
                  <div class="date-quick-buttons">
                    <v-btn-group density="compact" class="date-btn-group">
                      <v-btn 
                        v-for="preset in datePresets" 
                        :key="preset.value"
                        :class="['date-btn', { 'active-date-btn': dateRange === preset.value }]"
                        @click="setDateRange(preset.value)"
                        size="small"
                      >
                        {{ preset.label }}
                      </v-btn>
                    </v-btn-group>
                  </div>
                </div>
              </td>
              <!-- 버튼 영역 -->
              <td class="input-cell button-cell">
                <div class="action-buttons">
                  <v-btn 
                    variant="outlined" 
                    color="grey-darken-1" 
                    size="small" 
                    class="action-btn reset-btn"
                    @click="resetFilters"
                  >
                    <v-icon size="small" class="mr-1">mdi-refresh</v-icon>
                    초기화
                  </v-btn>
                  <v-btn 
                    variant="flat" 
                    color="primary" 
                    size="small" 
                    class="action-btn search-btn"
                    @click="handleSearch"
                  >
                    <v-icon size="small" class="mr-1">mdi-magnify</v-icon>
                    조회
                  </v-btn>
                </div>
              </td>
            </tr>

            <!-- 동적 필터 행들 -->
            <tr v-for="(row, rowIndex) in filterRows" :key="'row-' + rowIndex" class="filter-row">
              <template v-for="(filter, colIndex) in row" :key="'filter-' + rowIndex + '-' + colIndex">
                <!-- 라벨 셀 -->
                <td class="label-cell">
                  <span class="filter-label">{{ filter.label }}</span>
                </td>
                
                <!-- 입력 셀 -->
                <td class="input-cell" :class="getInputCellClass(row, colIndex)">
                  <!-- 제목 필터 -->
                  <v-text-field 
                    v-if="filter.type === 'title'"
                    v-model="localSubject" 
                    @keydown.enter="handleSearch" 
                    variant="outlined" 
                    density="compact" 
                    hide-details
                    class="table-input" 
                    placeholder="제목 입력"
                  />
                  
                  <!-- 법인 필터 -->
                  <v-select 
                    v-else-if="filter.type === 'company'"
                    v-model="localSelectedCompany" 
                    :items="companyOptions" 
                    item-title="text" 
                    item-value="value"
                    variant="outlined" 
                    density="compact" 
                    hide-details 
                    class="table-input" 
                    placeholder="법인 선택"
                  />
                  
                  <!-- 진행상태 필터 -->
                  <v-select 
                    v-else-if="filter.type === 'status'"
                    v-model="localSelectedStatus" 
                    :items="statusOptions" 
                    item-title="text" 
                    item-value="value"
                    variant="outlined" 
                    density="compact" 
                    hide-details 
                    class="table-input" 
                    placeholder="상태 선택"
                  />
                  
                  <!-- 담당자 필터 -->
                  <v-text-field 
                    v-else-if="filter.type === 'manager'"
                    v-model="localManager" 
                    @keydown.enter="handleSearch" 
                    variant="outlined" 
                    density="compact" 
                    hide-details
                    class="table-input" 
                    placeholder="담당자 입력"
                  />
                </td>
              </template>
              
              <!-- 빈 셀 채우기 (홀수 개일 때) -->
              <td v-if="row.length === 1" class="empty-cell" colspan="2"></td>
            </tr>          
          </tbody>
        </v-table>

      </div>
    </v-expand-transition>
  </v-card>
</template>

<script>
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css';

export default {
  name: 'DynamicSearchFilter',
  components: {
    VueDatePicker
  },
  props: {
    showManagerFilter: {
      type: Boolean,
      default: true
    },
    showStatusFilter: {
      type: Boolean,
      default: true
    },    
    showTitleFilter: {
      type: Boolean,
      default: true
    },       
    showCompanyFilter: {
      type: Boolean,
      default: true
    },         
    initialExpanded: {
      type: Boolean,
      default: true
    },
    propsDateRange: {
      type: String,
      default: 'month'
    },      
  },
  emits: ['search'],
  data() {
    return {
      isExpanded: this.initialExpanded,
      localStartDate: new Date(),
      localEndDate: new Date(),
      localSelectedStatus: '%',
      localSelectedCompany: '%',
      localManager: '',
      localSubject: '',
      dateRange: 'month',
      statusOptions: [],
      companyOptions: [],
      datePresets: [
        { label: '오늘', value: 'today' },
        { label: '1주일', value: 'week' },
        { label: '15일', value: '15days' },
        { label: '1개월', value: 'month' },
        { label: '3개월', value: '3months' }
      ]
    }
  },
  computed: {
    // 표시할 필터들을 순서대로 배열로 만들기
    availableFilters() {
      const filters = [];
      
      if (this.showTitleFilter) {
        filters.push({ type: 'title', label: '제목' });
      }
      if (this.showCompanyFilter) {
        filters.push({ type: 'company', label: '법인' });
      }
      if (this.showStatusFilter) {
        filters.push({ type: 'status', label: '진행상태' });
      }
      if (this.showManagerFilter) {
        filters.push({ type: 'manager', label: '담당자' });
      }
      
      return filters;
    },
    
    // 필터들을 2개씩 묶어서 행으로 만들기
    filterRows() {
      const rows = [];
      for (let i = 0; i < this.availableFilters.length; i += 2) {
        const row = this.availableFilters.slice(i, i + 2);
        rows.push(row);
      }
      return rows;
    }
  },
  mounted() {    
    console.log('this.propsDateRange->', this.propsDateRange);
    this.setDateRange(this.propsDateRange);
    this.getStatus();
    this.getCompany();
    this.handleSearch();
  },
  methods: {
    // 입력 셀의 클래스 결정
    // getInputCellClass(row, colIndex) {
      getInputCellClass(row) {
      // 행에 2개의 필터가 있으면 절반씩
      if (row.length === 2) {
        return 'half-width';
      }
      // 행에 1개의 필터만 있으면 절반만 차지
      return 'half-width';
    },

    async getStatus() {
      try {
        const apiClient = (await import('@/api')).default;
        
        const statusList = await apiClient.get("/api/code/list", {
          params: {
            category: 'STATUS'
          }
        });

        this.statusOptions = statusList.data.map(status => ({
          text: status.codeName,
          value: status.codeId
        }));

        this.statusOptions.unshift({ text: '전체', value: '%' });
        this.localSelectedStatus = '%';

      } catch (error) {
        console.error("❌ 상태 목록 조회 오류:", error);
        this.statusOptions = [{ text: '전체', value: '%' }];
        this.localSelectedStatus = '%';
      }
    },

    async getCompany() {
      try {
        const apiClient = (await import('@/api')).default;
        
        const statusList = await apiClient.get("/api/code/list", {
          params: {
            category: 'COMPANY'
          }
        });

        this.companyOptions = statusList.data.map(status => ({
          text: status.codeName,
          value: status.codeId
        }));

        this.companyOptions.unshift({ text: '전체', value: '%' });
        this.localSelectedCompany = '%';

      } catch (error) {
        console.error("❌ 상태 목록 조회 오류:", error);
        this.companyOptions = [{ text: '전체', value: '%' }];
        this.localSelectedCompany = '%';
      }
    },    

    onStartDateChange(date) {
      this.localStartDate = date;
      this.dateRange = ''; // 커스텀 선택 시 프리셋 해제
    },

    onEndDateChange(date) {
      this.localEndDate = date;
      this.dateRange = ''; // 커스텀 선택 시 프리셋 해제
    },

    setDateRange(range) {
      this.dateRange = range;
      const today = new Date();
      let start = new Date(today);

      switch (range) {
        case 'today':
          break;
        case 'week':
          start.setDate(today.getDate() - 7);
          break;
        case '15days':
          start.setDate(today.getDate() - 15);
          break;
        case 'month':
          start.setMonth(today.getMonth() - 1);
          break;
        case '3months':
          start.setMonth(today.getMonth() - 3);
          break;
      }

      this.localStartDate = start;
      this.localEndDate = today;
    },

    formatDate(dateObj) {
      if (!dateObj) return '';
      const year = dateObj.getFullYear();
      const month = String(dateObj.getMonth() + 1).padStart(2, '0');
      const day = String(dateObj.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    },

    handleSearch() {
      const searchParams = {
        startDate: this.formatDate(this.localStartDate) + ' 00:00:00',
        endDate: this.formatDate(this.localEndDate) + ' 23:59:59',
        status: this.localSelectedStatus,
        manager: this.localManager,
        sub: this.localSubject,
        division: this.localSelectedCompany
      };

      // console.log('--handleSearch--');
      // console.log(searchParams);

      this.$emit('search', searchParams);
    },

    resetFilters() {
      this.setDateRange('month');
      this.localSelectedStatus = '%';
      this.localSelectedCompany = '%';
      this.localManager = '';
      this.localSubject = '';
      // this.handleSearch();
    }
  }
}
</script>

<style scoped>
.search-container {
  border: 1px solid #D0DFF1;
  border-radius: 8px;
  background-color: rgba(208, 223, 241, 0.5);
  margin-bottom: 16px;
}

.search-content {
  padding: 12px;
}

/* 테이블 스타일 */
.search-table {
  background-color: white;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #D0DFF1;
}

.search-table :deep(table) {
  border-collapse: separate;
  border-spacing: 0;
}

.search-table :deep(tbody tr) {
  border-bottom: 1px solid #e0e0e0;
}

.search-table :deep(tbody tr:last-child) {
  border-bottom: none;
}

/* 셀 스타일 */
.label-cell {
  width: 100px;
  min-width: 150px;
  background-color: #f8f9fa;
  border-right: 1px solid #e0e0e0;
  padding: 12px 16px !important;
  vertical-align: middle;
}

.input-cell {
  padding: 8px 12px !important;
  vertical-align: middle;
}

.half-width {
  width: 50%;
}

.date-cell {
  padding: 12px !important;
}

.button-cell {
  min-width: 200px;
  width: 200px;
  text-align: right;
}

/* 라벨 스타일 */
.filter-label {
  font-size: 14px;
  font-weight: 600;
  color: #005bac;
  white-space: nowrap;
}

/* 날짜 관련 스타일 */
.date-section {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.date-input-section {
  display: flex;
  align-items: center;
  gap: 8px;
}

.date-picker-wrapper {
  width: 150px;
  flex: 0 0 150px;
}

.date-separator {
  font-size: 16px;
  color: #7A7A7A;
  font-weight: 500;
  transform: translateX(-12px);
  z-index: 1000;
}

.date-quick-buttons {
  flex: 1;
  min-width: 300px;
}

.date-btn-group {
  border-radius: 6px;
  overflow: hidden;
}

.date-btn {
  min-width: 50px;
  height: 32px;
  font-size: 13px;
  letter-spacing: -0.5px;
  border: 1px solid #eaeaea;
  background-color: #ffffff;
  color: #7A7A7A;
  text-transform: none;
}

.date-btn:hover {
  background-color: #f9f9f9;
}

.active-date-btn {
  background-color: #e8f4fd !important;
  color: #2196F3 !important;
  border-color: #2196F3 !important;
  font-weight: 500;
}

/* 입력 필드 스타일 */
.table-input {
  min-width: 120px;
}

.table-input :deep(.v-input__control) {
  min-height: 36px;
  height: 36px;
}

.table-input :deep(.v-field__input) {
  padding-top: 0;
  padding-bottom: 0;
  min-height: 36px;
  color: #5271C1;
  font-size: 14px;
}

.table-input :deep(.v-field) {
  border-radius: 6px;
}

/* 액션 버튼 */
.action-buttons {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.action-btn {
  height: 32px;
  border-radius: 6px;
  font-weight: 500;
  text-transform: none;
  min-width: 70px;
  font-size: 13px;
}

.search-btn {
  background: linear-gradient(135deg, #2196F3 0%, #1976D2 100%);
}

/* VueDatePicker 스타일 */
.date-picker {
  width: 100%;
  min-width: 0;
  padding: 0;  
}

:deep(.dp__input) {
  border: none;
  box-shadow: none;
  color: #7a7a7a;
}

:deep(.dp__theme_light) {
  --dp-primary-color: #2196F3;
  --dp-border-radius: 8px;
}

/* 반응형 디자인 */
@media (max-width: 1200px) {
  .date-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .date-quick-buttons {
    min-width: auto;
    width: 100%;
  }
  
  .action-buttons {
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .search-table {
    font-size: 13px;
  }
  
  .label-cell {
    width: 80px;
    min-width: 60px;
    padding: 8px 12px !important;
  }
  
  .input-cell {
    padding: 6px 8px !important;
  }
  
  .button-cell {
    min-width: 140px;
    width: 140px;
  }
  
  .date-picker-wrapper {
    width: 130px;
    flex: 0 0 130px;
  }
  
  .table-input {
    min-width: 100px;
  }
  
  .table-input :deep(.v-input__control) {
    min-height: 32px;
    height: 32px;
  }
  
  .action-btn {
    height: 28px;
    min-width: 60px;
    font-size: 12px;
  }
  
  .date-btn {
    min-width: 44px;
    height: 28px;
    font-size: 12px;
  }
}

@media (max-width: 600px) {
  .search-content {
    padding: 8px;
  }
  
  .date-input-section {
    flex-direction: column;
    gap: 8px;
    width: 100%;
  }
  
  .date-separator {
    transform: rotate(90deg);
  }
  
  .date-picker-wrapper {
    width: 100%;
    flex: 1;
  }
  
  .action-buttons {
    flex-direction: column;
    gap: 4px;
  }
  
  .table-input {
    min-width: auto;
  }
  
  .button-cell {
    min-width: 120px;
    width: 120px;
  }
}
</style>