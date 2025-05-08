<template>
    <v-dialog v-model="dialogVisible" width="900px" persistent>
        <v-card style="background-color: white;">
            <v-card-title class="primary-bg white--text d-flex justify-space-between align-center">
                <span class="title-custom">담당자 검색</span>
                <v-icon @click="closeDialog" color="#155A9E">mdi-close</v-icon>
            </v-card-title>

            <v-card-text class="py-4">
                <v-row>
                    <v-col cols="12">
                        <v-text-field v-model="searchText" label="이름을 검색해주세요" append-icon="mdi-magnify" outlined dense
                            hide-details @keyup.enter="fetchData"></v-text-field>
                    </v-col>
                </v-row>

                <div class="table-scroll-wrapper">
                    <v-data-table :headers="headers" :items="users" :items-per-page="-1" hide-default-footer
                        class="mt-4 elevation-0" item-value="usrId" v-model="selectedUsers" @click:row="rowClick">
                        <template #[`item.select`]="{ item }">
                            <v-checkbox :model-value="selectedUsers.includes(item.usrId)" hide-details density="compact"
                                @click.stop @change="toggleSelection(item)" />
                        </template>

                        <template #no-data>
                            <div class="d-flex justify-center align-center pa-4 text-subtitle-1" style="color:#155A9E">
                                <v-icon class="mr-2">mdi-alert-circle-outline</v-icon>
                                담당자 이름을 검색해주세요.
                            </div>
                        </template>
                    </v-data-table>
                </div>
            </v-card-text>

            <v-card-actions class="d-flex justify-end pa-4 mr-2">
                <v-btn color="primary" @click="addUser()" :disabled="!selectedUser">
                    담당자 등록
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script>
import apiClient from '@/api';

export default {
    name: 'AdminPopup',
    props: {
        show: {
            type: Boolean,
            default: false
        },        
    },
    data() {
        return {
            dialogVisible: false,
            searchText: '',
            selectedUser: null,
            selectedUsers: [], // 체크박스 선택을 위한 배열
            headers: [
                { title: '', key: 'select', sortable: false, width: '50px' },
                { title: '법인', key: 'corpNm', align: 'start' },
                { title: '부서', key: 'deptNm', align: 'start' },
                { title: '이름', key: 'name', align: 'start' },
                { title: '직급', key: 'rollPstnNm', align: 'start' },
                { title: '아이디', key: 'usrId', align: 'start' },
            ],
            users: [],
        };
    },
    watch: {
        show(newVal) {
            this.dialogVisible = newVal;
            if (newVal) {
                this.resetForm();
            }
        },
        dialogVisible(newVal) {
            if (!newVal) {
                this.$emit('close');
            }
        }
    },
    methods: {
        handleSelection(item) {
            if (this.selectedUsers.includes(item)) {
                this.selectedUser = item;
                this.selectedUsers = this.selectedUsers.filter(user => user === item);
            } else {
                this.selectedUser = null;
                this.selectedUsers = [];
            }
        },
        addUser() {
            // 글작성 폼으로 전달
            this.$emit('manager-selected', this.selectedUser);
            // 글수정 폼으로 전달
            this.$emit('manager-selected_edit', this.selectedUser);
            this.$emit('manager-selected_sr_edit', this.selectedUser);
            this.$emit('manager-selected_detail_form', this.selectedUser);
            this.closeDialog();
        },
        closeDialog() {
            this.dialogVisible = false;
            this.users = [];
            this.selectedUser = null;
            this.selectedUsers = [];
        },
        resetForm() {
            this.selectedUser = null;
            this.selectedUsers = [];
            this.searchText = '';
        },
        async fetchData() {

            try {
                // 서버 측 페이징을 구현할 경우 페이지 관련 파라미터 추가
                const response = await apiClient.get('/api/userPop/list', {
                    params: {
                        name: this.searchText
                    }
                });

                // API 응답 데이터 처리
                if (response.data && Array.isArray(response.data)) {
                    this.users = response.data;
                } else {
                    this.tableData = [];
                }
            } catch (error) {
                console.error('데이터 로드 중 오류 발생:', error);
            } finally {
                this.loading = false;
            }
        },
        rowClick(event, { item }) {
            // 체크박스 상태 전환
            const index = this.selectedUsers.indexOf(item.usrId);

            // 모든 선택 해제
            this.selectedUsers = [];
            this.selectedUser = null;

            // 현재 항목 선택
            if (index === -1) {
                this.selectedUsers.push(item.usrId);
                this.selectedUser = item;
            }
        },
        toggleSelection(item) {
            // 이미 선택된 항목인지 확인
            const index = this.selectedUsers.indexOf(item.usrId);

            // 모든 선택 해제
            this.selectedUsers = [];
            this.selectedUser = null;

            // 선택되지 않은 상태였다면 선택
            if (index === -1) {
                this.selectedUsers.push(item.usrId);
                this.selectedUser = item;
            }
        }
    },
    created() {
        this.dialogVisible = this.show;
    },
    mounted() {
        this.fetchData();
    },
}
</script>

<style scoped>
.primary-bg {
    background-color: #B0CAE6 !important;
    height: 65px;
}

/* 헤더 스타일 수정 */
:deep(.v-data-table-header) {
    background-color: #f5f7fa !important;
}

:deep(.v-data-table-header th) {
    background-color: #f5f7fa !important;
    color: #333 !important;
    font-weight: 500 !important;
}

.title-custom {
    color: #155A9E;
    font-weight: 600;
}

.table-scroll-wrapper {
    height: 480px;
    overflow-y: auto;
}

:deep(.v-data-table) {
    user-select: none;
    cursor: pointer;
}
</style>