<template>
  <v-card 
    elevation="0" 
    class="search-container"
    :class="{ 'search-expanded': isExpanded }"
  >
    <!-- 검색 헤더 -->
    <div class="search-header" @click="toggleExpanded">
      <div class="search-header-content">
        <v-icon class="search-icon">mdi-filter-variant</v-icon>
        <span class="search-title">검색 조건</span>
        <!-- <v-chip 
          v-if="activeFiltersCount > 0" 
          color="primary" 
          size="small" 
          class="filter-count-chip"
        >
          {{ activeFiltersCount }}
        </v-chip> -->
      </div>
      <v-btn 
        :icon="isExpanded ? 'mdi-chevron-up' : 'mdi-chevron-down'"
        variant="text"
        size="small"
        class="expand-btn"
      />
    </div>

    <!-- 검색 필터 영역 -->
    <v-expand-transition>
      <div v-show="isExpanded" class="search-content">
        <v-row no-gutters class="filter-grid">
          
          <!-- 요청기간 (1/3 너비) -->
          <v-col cols="12" lg="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <v-icon size="18" class="filter-icon">mdi-calendar-range</v-icon>
                요청기간
                <div class="date-quick-buttons-inline">
                  <v-btn-group density="compact" class="date-btn-group-inline">
                    <v-btn 
                      v-for="preset in datePresets" 
                      :key="preset.value"
                      :variant="dateRange === preset.value ? 'flat' : 'outlined'"
                      :color="dateRange === preset.value ? 'primary' : 'default'"
                      size="x-small"
                      @click="setDateRange(preset.value)"
                      class="date-preset-btn-inline"
                    >
                      {{ preset.label }}
                    </v-btn>
                  </v-btn-group>
                </div>
              </div>
              <div class="date-range-container">
                <div class="date-input-group-compact">
                  <VueDatePicker 
                    v-model="localStartDate"
                    class="modern-date-picker"
                    :teleport="true"
                    position="bottom"
                    :enable-time-picker="false"
                    auto-apply
                    locale="ko"
                    format="MM-dd"
                    :clearable="false"
                    :text-input="false"
                    placeholder="시작일"
                    @update:model-value="onStartDateChange"
                  />
                  <span class="date-separator-compact">~</span>
                  <VueDatePicker 
                    v-model="localEndDate"
                    class="modern-date-picker"
                    :teleport="true"
                    position="bottom"
                    :enable-time-picker="false"
                    auto-apply
                    locale="ko"
                    format="MM-dd"
                    :clearable="false"
                    :text-input="false"
                    placeholder="종료일"
                    @update:model-value="onEndDateChange"
                  />
                </div>
              </div>
            </div>
          </v-col>

          <!-- 접수상태 (1/3 너비) -->
          <v-col v-if="showStatusFilter" cols="12" lg="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <div class="label-with-icon">
                  <v-icon size="18" class="filter-icon">mdi-format-list-bulleted</v-icon>
                  접수상태
                </div>
              </div>
              <v-select
                v-model="localSelectedStatus"
                :items="statusOptions"
                item-title="text"
                item-value="value"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-select"
                placeholder="상태 선택"
              >
                <template v-slot:selection="{ item }">
                  <v-chip 
                    v-if="localSelectedStatus && localSelectedStatus !== '%'"
                    color="primary" 
                    size="small"
                    class="selection-chip"
                  >
                    {{ item.title }}
                  </v-chip>
                </template>
              </v-select>
            </div>
          </v-col>

          <!-- 담당자 (1/3 너비) -->
          <v-col 
            v-if="showManagerFilter" 
            cols="12" 
            lg="4" 
            class="filter-item"
          >
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <div class="label-with-icon">
                  <v-icon size="18" class="filter-icon">mdi-account</v-icon>
                  담당자
                </div>
              </div>
              <v-text-field
                v-model="localManager"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-text-field"
                placeholder="담당자 입력"
                @keydown.enter="handleSearch"
                clearable
              />
            </div>
          </v-col>

          <!-- 제목 (1/3 너비) -->
          <v-col v-if="showTitleFilter" cols="12" lg="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <div class="label-with-icon">
                  <v-icon size="18" class="filter-icon">mdi-text-search</v-icon>
                  제목
                </div>
              </div>
              <v-text-field
                v-model="localSubject"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-text-field"
                placeholder="제목 입력"
                @keydown.enter="handleSearch"
                clearable
              />
            </div>
          </v-col>

          <!-- 추가 검색조건 예시 (필요시 활용) -->
          <!-- 
          <v-col cols="12" md="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <v-icon size="18" class="filter-icon">mdi-domain</v-icon>
                부서
              </div>
              <v-select
                v-model="localDepartment"
                :items="departmentOptions"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-select"
                placeholder="부서 선택"
              />
            </div>
          </v-col>

          <v-col cols="12" md="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <v-icon size="18" class="filter-icon">mdi-star</v-icon>
                우선순위
              </div>
              <v-select
                v-model="localPriority"
                :items="priorityOptions"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-select"
                placeholder="우선순위 선택"
              />
            </div>
          </v-col>

          <v-col cols="12" md="4" class="filter-item">
            <div class="filter-wrapper">
              <div class="filter-label-modern">
                <v-icon size="18" class="filter-icon">mdi-tag</v-icon>
                카테고리
              </div>
              <v-text-field
                v-model="localCategory"
                variant="outlined"
                density="comfortable"
                hide-details
                class="modern-text-field"
                placeholder="카테고리 입력"
                @keydown.enter="handleSearch"
                clearable
              />
            </div>
          </v-col>
          -->

        </v-row>

        <!-- 액션 버튼 영역 -->
        <div class="action-section">
          <div class="action-buttons">
            <v-btn
              variant="outlined"
              color="grey-darken-1"
              class="reset-btn"
              @click="resetFilters"
              prepend-icon="mdi-refresh"
            >
              초기화
            </v-btn>
            <v-btn
              variant="flat"
              color="primary"
              class="search-btn"
              @click="handleSearch"
              prepend-icon="mdi-magnify"
            >
              검색
            </v-btn>
          </div>
        </div>

        <!-- 활성 필터 태그 -->
        <!-- <div v-if="activeFilterTags.length > 0" class="active-filters">
          <div class="active-filters-label">활성 필터:</div>
          <v-chip
            v-for="tag in activeFilterTags"
            :key="tag.key"
            color="primary"
            variant="tonal"
            size="small"
            closable
            class="filter-tag"
            @click:close="clearFilter(tag.key)"
          >
            {{ tag.label }}: {{ tag.value }}
          </v-chip>
        </div> -->
      </div>
    </v-expand-transition>
  </v-card>
</template>


<style scoped>
.search-container {
  border-radius: 12px;
  border: 1px solid #e0e7ff;
  background: linear-gradient(135deg, #f8faff 0%, #f1f5ff 100%);
  transition: all 0.3s ease;
  margin-bottom: 24px;
}

.search-container.search-expanded {
  box-shadow: 0 8px 25px rgba(59, 130, 246, 0.15);
}

.search-header {
  padding: 16px 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: space-between;
  transition: all 0.2s ease;
}

.search-header:hover {
  background-color: rgba(59, 130, 246, 0.05);
}

.search-header-content {
  display: flex;
  align-items: center;
  gap: 12px;
}

.search-icon {
  color: #3b82f6;
}

.search-title {
  font-size: 16px;
  font-weight: 600;
  color: #1e293b;
}

.filter-count-chip {
  font-weight: 600;
}

.expand-btn {
  color: #64748b;
}

.search-content {
  padding: 0 20px 20px;
}

.filter-grid {
  gap: 16px;
  margin-bottom: 20px;
}

.filter-item {
  padding: 8px;
}

.filter-wrapper {
  background: white;
  border-radius: 12px;
  padding: 16px;
  border: 1px solid #e2e8f0;
  transition: all 0.2s ease;
  height: 100%;
}

.filter-wrapper:hover {
  border-color: #3b82f6;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
}

.filter-label-modern {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.label-with-icon {
  display: flex;
  align-items: center;
  gap: 6px;
}

.date-quick-buttons-inline {
  margin-left: 16px;
}

.date-btn-group-inline {
  border-radius: 6px;
  overflow: hidden;
}

.date-preset-btn-inline {
  font-size: 10px;
  text-transform: none;
  letter-spacing: 0;
  min-width: auto;
  padding: 0 6px;
  height: 24px;
}

.filter-icon {
  color: #3b82f6;
}

.date-range-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.date-input-group-compact {
  display: flex;
  align-items: center;
  gap: 8px;
}

.date-separator-compact {
  color: #64748b;
  font-weight: 500;
  font-size: 14px;
  flex-shrink: 0;
}

.date-quick-buttons-compact {
  display: flex;
  justify-content: center;
  margin-top: 8px;
}

.date-btn-group-compact {
  border-radius: 6px;
  overflow: hidden;
}

.date-preset-btn-compact {
  font-size: 10px;
  text-transform: none;
  letter-spacing: 0;
  min-width: auto;
  padding: 0 8px;
}

.date-input-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.modern-date-picker {
  flex: 1;
}

.date-separator {
  color: #64748b;
  font-weight: 500;
  font-size: 16px;
}

.date-quick-buttons {
  display: flex;
  justify-content: center;
}

.date-btn-group {
  border-radius: 8px;
  overflow: hidden;
}

.date-preset-btn {
  font-size: 12px;
  text-transform: none;
  letter-spacing: 0;
}

.modern-select, .modern-text-field {
  background-color: #f8fafc;
  border-radius: 8px;
}

.selection-chip {
  font-size: 12px;
}

.action-section {
  border-top: 1px solid #e2e8f0;
  padding-top: 20px;
  margin-top: 20px;
}

.action-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.reset-btn, .search-btn {
  height: 40px;
  border-radius: 8px;
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
}

.search-btn {
  min-width: 120px;
}

.active-filters {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.active-filters-label {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
}

.filter-tag {
  font-size: 12px;
}

/* VueDatePicker 스타일 커스터마이징 */
:deep(.dp__input) {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 10px 12px;
  font-size: 14px;
  background-color: #f8fafc;
  transition: all 0.2s ease;
}

:deep(.dp__input:hover) {
  border-color: #3b82f6;
}

:deep(.dp__input:focus) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

:deep(.dp__main) {
  font-family: inherit;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
}

:deep(.dp__theme_light) {
  --dp-primary-color: #3b82f6;
  --dp-border-radius: 12px;
}

/* 반응형 디자인 */
@media (max-width: 768px) {
  
  .date-input-group {
    flex-direction: column;
    gap: 8px;
  }
  
  .date-separator {
    transform: rotate(90deg);
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .active-filters {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>