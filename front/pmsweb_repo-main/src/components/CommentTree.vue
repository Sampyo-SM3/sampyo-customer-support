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
        <div class="comment-text">{{ comment.content }}</div>
        <v-btn text class="reply-btn" @click="toggleReplyInput(comment)">답글</v-btn>
        <v-btn text class="reply-btn" @click="deleteComment(comment.commentId)">삭제</v-btn>

        <!-- ✅ 답글 입력창을 댓글과 같은 div 안에 배치 -->
        <div v-if="showReplyInput" class="reply-input-container">
          <v-textarea v-model="replyContent" label="답글 입력" class="reply-textarea"></v-textarea>
          <div class="reply-btn-container">
            <v-btn text class="reply-sub-btn" @click="submitReply">등록</v-btn>
            <v-btn text class="reply-sub-btn" @click="cancelReply">취소</v-btn>
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
    }
  },
  data() {
    return {
      showReplyInput: false,  // 답글 입력창 표시 여부
      replyContent: "",        // 답글 입력 내용
      replyParent: {}
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
    }
  },
  methods: {
    toggleReplyInput(cmt) {
      this.replyParent = cmt;
      this.showReplyInput = !this.showReplyInput;  // 클릭할 때마다 입력창 표시/숨김

      console.log(this.userInfo);
    },
    async submitReply() {
      if (!this.replyContent.trim()) {
        alert("댓글을 입력해주세요.");
        return;
      }

      // 부모 댓글 여부 확인 후 parentId 설정
      let newParentId = this.replyParent ? this.replyParent.commentId : null;
      let newDepth = this.replyParent ? this.replyParent.depth + 1 : 0;  // 대댓글이면 부모 depth + 1

      // DB에 저장할 댓글 데이터
      const commentData = {
        postId: this.replyParent.postId || 1,  // 게시글 ID
        userId: "test_user",  // 유저 ID
        content: this.replyContent,  // 댓글 내용
        parentId: newParentId,  // 부모 댓글 ID (없으면 NULL)
        depth: newDepth,  // 대댓글이면 부모 depth + 1, 최상위 댓글이면 0
        createdAt: new Date().toISOString(),  // 현재 시간
        deleteYn: "N"  // 삭제 여부 (기본값 "N")
      };

      try {
        // API 요청: 댓글 DB에 저장
        await apiClient.post("api/insertComment", commentData);
        alert("댓글 등록 성공!");

      } catch (error) {
        console.error("❌ 댓글 등록 실패");
        alert("댓글 등록 실패!");

      } finally {
        // ✅ 입력 필드 초기화 & 대댓글 입력창 닫기
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
      return dateStr.split('.').join('-');
    },
    async deleteComment(commentId) {
      if (confirm("댓글을 삭제하시겠습니까?")) {
        try {
          await apiClient.post(`http://localhost:8080/api/deleteComment/${commentId}`);
          alert("댓글이 삭제되었습니다.");
        } catch (error) {
          console.log(error);
          alert("삭제를 실패하였습니다. 관리자에게 문의하세요.");
        }
      }
      this.$emit("refresh");
    }
  }
};
</script>

<style scoped>
.comment-wrapper {
  margin-bottom: 5px;
}

.comment-item {
  padding: 15px;
  border-bottom: 1px solid #e0e0e0;
  display: flex;
  align-items: flex-start;
}

.comment-item.has-reply-input {
  border-bottom: none !important;
}

.reply-indicator-wrapper {
  display: flex;
  align-items: center;
}

.reply-indicator {
  color: #666;
  margin-right: 8px;
  font-weight: bold;
  font-size: 16px;
}

.comment-content {
  flex: 1;
}

.comment-header {
  margin-bottom: 8px;
}

.comment-user {
  font-weight: 500;
  margin-right: 10px;
}

.comment-date {
  color: #666;
  font-size: 13px;
}

.comment-text {
  margin: 5px 0;
  font-size: 14px;
  line-height: 1.4;
}

.reply-btn {
  text-transform: none !important;
  min-width: 30px !important;
  height: 28px !important;
  font-size: 12px !important;
  color: #666 !important;
  margin-top: 5px;
  margin-left: 5px;
  box-shadow: none !important;
  background-color: transparent !important;
  border: 1px solid #ddd !important;
  border-radius: 4px !important;
}

.reply-sub-btn {
  text-transform: none !important;
  min-width: 45px !important;
  height: 30px !important;
  font-size: 14px !important;
  color: #666 !important;
  margin-top: 3px;
  margin-left: 5px;
  box-shadow: none !important;
  background-color: transparent !important;
  border: 1px solid #ddd !important;
  border-radius: 4px !important;
}

.reply-input-container {
  margin-top: 8px;
  padding: 10px;
  border-left: none;
  border-radius: 6px;
}

.reply-textarea {
  width: 100%;
  font-size: 14px;
  min-height: 60px;
}

.reply-btn-container {
  display: flex;
  justify-content: flex-end;
  gap: 5px;
}

.reply-submit-btn {
  background-color: #1867C0;
  color: white;
}

.reply-cancel-btn {
  background-color: #ccc;
  color: white;
}

.replies-container {
  width: 100%;
}
</style>
