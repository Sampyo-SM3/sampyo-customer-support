<template>
  <div class="comment-wrapper">
    <div class="comment-item">
      <div v-if="comment.depth > 0" class="reply-indicator-wrapper" :style="getIndentStyle">
        <span class="reply-indicator">ㄴ</span>
      </div>
      <div class="comment-content">
        <div class="comment-header">
          <span class="comment-user">{{ comment.userId }}</span>
          <span class="comment-date">{{ formatDate(comment.createdAt) }}</span>
        </div>

        <!-- 수정모드가 아닐때 -->
        <div v-if="!isEditing" class="comment-text">{{ comment.content }}</div>

        <!-- 수정 모드일 때 -->
        <div v-else class="comment-edit-container">
          <v-textarea auto-grow v-model="editedContent" label="댓글 수정" variant="outlined" density="comfortable"
            color="#3A70B1" rows="3" hide-details class="edit-textarea"></v-textarea>
          <div class="edit-btn-container">
            <!-- <v-btn text class="edit-btn" color="#666" @click="cancelEdit">            
              취소
            </v-btn> -->
            <v-btn variant="flat" color="#666" class="white--text save-btn" @click="cancelEdit">
              취소
            </v-btn>
            <v-btn variant="flat" color="#3A70B1" class="white--text save-btn" @click="saveEditedComment(comment)">
              저장
            </v-btn>
          </div>
        </div>

        <div class="comment-actions" v-if="comment.userId === this.userId">
          <v-menu>
            <template v-slot:activator="{ isActive, props }">
              <v-icon v-bind="props" :color="isActive ? '#3A70B1' : '#666'" size="18">
                mdi-dots-vertical
              </v-icon>
            </template>

            <v-list class="more-actions-list">
              <v-list-item @click="onEdit" title="수정" class="more-actions-item" />
              <v-list-item @click="deleteComment(comment.commentId)" title="삭제" class="more-actions-item" />
            </v-list>
          </v-menu>
        </div>

        <div class="comment-footer">
          <v-btn v-if="!isEditing" variant="text" size="small" class="reply-btn" @click="toggleReplyInput(comment)">
            <v-icon size="16" class="mr-1">mdi-reply</v-icon>
            답글
          </v-btn>
        </div>

        <div v-if="showReplyInput" class="reply-input-container">
          <v-textarea auto-grow v-model="replyContent" label="답글 입력" variant="outlined" density="comfortable"
            color="#3A70B1" rows="2" hide-details class="reply-textarea"></v-textarea>
          <div class="reply-btn-container">
            <!-- <v-btn text class="cancel-reply-btn" color="#666" @click="cancelReply">
              취소
            </v-btn> -->
            <v-btn variant="flat" color="#666" class="white--text submit-reply-btn" @click="cancelReply">
              취소
            </v-btn>
            <v-btn variant="flat" color="#3A70B1" class="white--text submit-reply-btn" @click="submitReply">
              등록
            </v-btn>
          </div>
        </div>
      </div>
    </div>

    <!-- 대댓글 목록 -->
    <div class="replies-container">
      <comment-tree v-for="child in childComments" :key="child.commentId" :comment="child" :all-comments="allComments"
        @reply="$emit('reply', $event)" @delete="$emit('delete', $event)" @refresh="$emit('refresh')" />
    </div>
  </div>
</template>

<script>
import apiClient from '@/api';

export default {
  name: 'CommentTree',
  props: {
    comment: {
      type: Object,
      required: true
    },
    allComments: {
      type: Array,
      required: true
    },

  },
  data() {
    return {
      showReplyInput: false,  // 답글 입력창 표시 여부
      replyContent: "",        // 답글 입력 내용
      replyParent: {},
      userId: null,            // 사용자 ID 변수 추가
      userName: null,
      isEditing: false,
      editedContent: '',
    };
  },
  computed: {
    childComments() {
      return this.allComments.filter(c => c.parentId === this.comment.commentId);
    },
    getIndentStyle() {
      return {
        marginLeft: `${(this.comment.depth - 1) * 20}px`
      };
    },
  },
  created() {
    // 컴포넌트가 생성될 때 로컬 스토리지에서 사용자 정보 가져오기
    this.getUserInfoFromLocalStorage();
  },
  // 페이지 이동 시에도 사용자 정보를 가져오도록 설정
  beforeRouteEnter(to, from, next) {
    next(vm => {
      vm.getUserInfoFromLocalStorage();
    });
  },
  // 라우트가 업데이트될 때도 사용자 정보를 가져오도록 설정
  beforeRouteUpdate(to, from, next) {
    this.getUserInfoFromLocalStorage();
    next();
  },
  methods: {
    getUserInfoFromLocalStorage() {
      // 로컬 스토리지에서 사용자 정보 가져와서 userId에 설정
      this.userId = JSON.parse(localStorage.getItem("userInfo"))?.id || null;
      this.userName = JSON.parse(localStorage.getItem("userInfo"))?.name || null;
    },
    toggleReplyInput(cmt) {
      this.replyParent = cmt;
      this.showReplyInput = !this.showReplyInput;  // 클릭할 때마다 입력창 표시/숨김

      // 토글 시 최신 사용자 정보 다시 가져오기
      this.getUserInfoFromLocalStorage();
    },
    async submitReply() {
      if (!this.replyContent.trim()) {
        alert("댓글을 입력해주세요.");
        return;
      }

      // 사용자 ID가 없으면 알림
      if (!this.userId) {
        alert("로그인이 필요합니다.");
        return;
      }

      // 부모 댓글 여부 확인 후 parentId 설정
      let newParentId = this.replyParent ? this.replyParent.commentId : null;
      let newDepth = this.replyParent ? this.replyParent.depth + 1 : 0;  // 대댓글이면 부모 depth + 1

      // DB에 저장할 댓글 데이터
      const commentData = {
        postId: this.replyParent.postId || 1,  // 게시글 ID
        userId: this.userId,  // 유저 ID 변경
        content: this.replyContent,  // 댓글 내용
        parentId: newParentId,  // 부모 댓글 ID (없으면 NULL)
        depth: newDepth,  // 대댓글이면 부모 depth + 1, 최상위 댓글이면 0
        createdAt: new Date().toISOString(),  // 현재 시간
        deleteYn: "N"  // 삭제 여부 (기본값 "N")
      };

      try {
        // API 요청: 댓글 DB에 저장
        await apiClient.post("api/insertComment", commentData);

      } catch (error) {
        alert("오류가 발생했습니다. 관리자에게 문의해주세요.");
      } finally {
        // 입력 필드 초기화 & 대댓글 입력창 닫기
        this.$emit("refresh");
        this.replyContent = "";
        this.showReplyInput = false;
        this.replyParent = null;
      }
    },
    cancelReply() {
      this.replyContent = "";
      this.showReplyInput = false;
    },
    formatDate(dateStr) {
      if (!dateStr) return '-';

      const date = new Date(dateStr);

      if (isNaN(date)) return '-';

      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      const seconds = String(date.getSeconds()).padStart(2, '0');

      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    },
    async deleteComment(commentId) {
      // 사용자 ID 확인 (최신 정보로 갱신)
      this.getUserInfoFromLocalStorage();

      if (confirm("댓글을 삭제하시겠습니까?")) {
        try {
          await apiClient.post(`/api/deleteComment/${commentId}`);
        } catch (error) {
          alert("오류가 발생했습니다. 관리자에게 문의해주세요.");
        }
      }
      this.$emit("refresh");
    },
    onEdit() {
      this.isEditing = true;
      this.editedContent = this.comment.content;
    },
    cancelEdit() {
      this.isEditing = false;
      this.editedContent = this.comment.content;
    },
    async saveEditedComment(obj) {
      if (!obj.content.trim()) {
        alert("댓글을 입력해주세요.");
        return;
      }

      obj.content = this.editedContent;

      const commentData = {
        commentId: obj.commentId,  // 게시글 ID
        userId: this.userName,  // 유저 ID 변경
        content: obj.content  // 댓글 내용
      };

      try {
        await apiClient.post("api/updateComment", commentData);
      } catch (error) {
        alert("오류가 발생하였습니다. 관리자에게 문의해주세요.");

      } finally {
        // ✅ 입력 필드 초기화 & 대댓글 입력창 닫기
        this.$emit("refresh");
        this.replyContent = "";
        this.showReplyInput = false;
        this.replyParent = null;
      }

      this.isEditing = false;
    },
  }
};
</script>

<style scoped>
.comment-wrapper {
  margin-bottom: 12px;
}

.comment-item {
  position: relative;
  display: flex;
  flex-direction: row;
  padding: 12px 16px;
  background-color: white;
  border-radius: 6px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  margin-bottom: 8px;
}

.reply-indicator-wrapper {
  display: flex;
  align-items: center;
}

.reply-indicator {
  color: #3A70B1;
  margin-right: 8px;
  font-weight: bold;
  font-size: 16px;
}

.comment-content {
  flex: 1;
}

.comment-header {
  margin-bottom: 8px;
  display: flex;
  align-items: center;
}

.comment-user {
  font-weight: 500;
  margin-right: 10px;
  color: #333;
  font-size: 14px;
}

.comment-date {
  color: #888;
  font-size: 12px;
}

.comment-text {
  margin: 5px 0 10px;
  font-size: 14px;
  line-height: 1.5;
  color: #444;
  white-space: pre-wrap;
  /* 엔터와 공백을 그대로 표시 */
  word-break: break-word;
  /* 긴 단어가 있을 때 줄바꿈 */
}

.comment-footer {
  display: flex;
  align-items: center;
  margin-top: 5px;
}

.reply-btn {
  font-size: 12px !important;
  color: #666 !important;
  text-transform: none !important;
  min-width: auto !important;
  height: 28px !important;
  padding: 0 10px !important;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.comment-actions {
  position: absolute;
  right: 12px;
  top: 12px;
}

.more-actions-list {
  min-width: 100px;
  border-radius: 4px;
}

.more-actions-item {
  font-size: 14px !important;
  min-height: 36px;
}

.reply-input-container {
  margin-top: 12px;
  padding: 10px;
  background-color: #f5f8fb;
  border-radius: 4px;
}

.reply-textarea,
.edit-textarea {
  width: 100%;
  font-size: 14px;
  background-color: white;
  border-radius: 4px;
}

.reply-btn-container,
.edit-btn-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 10px;
  gap: 8px;
}


.submit-reply-btn,
.save-btn {
  font-size: 13px;
  text-transform: none;
  height: 32px;
  border-radius: 4px;
  color: white !important;
}

.replies-container {
  padding-left: 24px;
}

.comment-edit-container {
  margin-top: 10px;
  margin-bottom: 10px;
}
</style>