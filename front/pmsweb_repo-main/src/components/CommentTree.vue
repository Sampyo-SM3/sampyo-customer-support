<template>
    <div class="comment-wrapper">
      <div class="comment-item">
        <div v-if="comment.depth > 0" class="reply-indicator-wrapper" :style="getIndentStyle">
          <span class="reply-indicator">ㄴ</span>
        </div>
        <div class="comment-content">
          <div class="comment-header">
            <span class="comment-user">{{ comment.userId }}</span>
            <span class="comment-date">{{ formatDate(comment.formattedCreatedAt) }}</span>
          </div>
          <div class="comment-text">{{ comment.content }}</div>
          <v-btn text class="reply-btn" @click="$emit('reply', comment)">답글</v-btn>
        </div>
      </div>
      
      <div class="replies-container">
        <comment-tree
          v-for="child in childComments"
          :key="child.commentId"
          :comment="child"
          :all-comments="allComments"
          @reply="$emit('reply', $event)"
        />
      </div>
    </div>
  </template>
  
  <script>
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
      formatDate(dateStr) {
        return dateStr.split('.').join('-');
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
    min-width: 50px !important;
    height: 28px !important;
    font-size: 13px !important;
    color: #666 !important;
    margin-top: 5px;
  }
  
  .replies-container {
    width: 100%;
  }
  </style>