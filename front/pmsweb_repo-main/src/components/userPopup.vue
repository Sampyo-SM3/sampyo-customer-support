<template>
    <v-dialog v-model="dialogVisible" width="900px" persistent style="border: 5px solid red;">
        <v-card>
            <v-card-title class="primary-bg white--text d-flex justify-space-between align-center">
                <span class="title-custom">임직원 검색</span>
                <v-icon @click="closeDialog" color="#155A9E">mdi-close</v-icon>
            </v-card-title>

            <v-card-text>
                <!-- 검색 및 테이블 생략 -->
            </v-card-text>

            <v-card-actions class="d-flex justify-end pa-4 mr-2">
                <v-btn color="primary" @click="addUser" :disabled="!selectedUser">등록</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script>
export default {
    name: 'UserPopup',
    props: {
        show: {
            type: Boolean,
            required: true
        }
    },
    data() {
        return {
            searchText: '',
            users: [],
            selectedUser: null,
            selectedUsers: [],
        };
    },
    computed: {
        computed: {
            dialogVisible: {
                get() {
                    console.log('🔍 show (from parent):', this.show);
                    return this.show;
                },
                set(val) {
                    console.log('🔁 dialogVisible set:', val);
                    if (!val) {
                        this.$emit('close');
                    }
                }
            }
        }

    },
    methods: {
        closeDialog() {
            this.dialogVisible = false; // computed setter가 emit 해줌
        },
        addUser() {
            this.$emit('manager-selected', this.selectedUser);
            this.closeDialog();
        }
    }
};
</script>