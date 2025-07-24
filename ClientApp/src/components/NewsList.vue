<template>
  <div class="news-list">
    <!-- 頁面標題 -->
    <div class="page-header">
      <div class="header-left">
        <h2 class="page-title">最新消息管理</h2>
        <p class="page-description">管理網站最新消息和重要公告</p>
      </div>
      <div class="header-actions">
        <router-link to="/dashboard/news/create" class="create-btn">
          📢 發布消息
        </router-link>
      </div>
    </div>

    <!-- 篩選和搜尋 -->
    <div class="filters">
      <div class="search-box">
        <input
            v-model="searchQuery"
            type="text"
            placeholder="搜尋消息標題..."
            class="search-input"
        />
      </div>
      <div class="filter-buttons">
        <button
            @click="statusFilter = 'all'"
            :class="{ active: statusFilter === 'all' }"
            class="filter-btn"
        >
          全部
        </button>
        <button
            @click="statusFilter = 'published'"
            :class="{ active: statusFilter === 'published' }"
            class="filter-btn"
        >
          已發布
        </button>
        <button
            @click="statusFilter = 'draft'"
            :class="{ active: statusFilter === 'draft' }"
            class="filter-btn"
        >
          草稿
        </button>
        <button
            @click="statusFilter = 'important'"
            :class="{ active: statusFilter === 'important' }"
            class="filter-btn important"
        >
          重要消息
        </button>
      </div>
      <div class="actions">
        <button @click="refreshData" class="refresh-btn" :disabled="loading">
          🔄 重新整理
        </button>
      </div>
    </div>

    <!-- 消息列表 -->
    <div class="news-container">
      <div v-if="loading" class="loading">
        <div class="loading-spinner"></div>
        <p>載入中...</p>
      </div>

      <div v-else-if="filteredNews.length === 0" class="empty-state">
        <div class="empty-icon">📰</div>
        <p v-if="searchQuery">找不到符合條件的消息</p>
        <p v-else>暫無消息，<router-link to="/dashboard/news/create">立即發布第一則消息</router-link></p>
      </div>

      <div v-else class="news-cards">
        <div
            v-for="news in paginatedNews"
            :key="news.id"
            class="news-card"
            :class="{ important: news.isImportant }"
        >
          <!-- 重要標記 -->
          <div v-if="news.isImportant" class="important-badge">
            ⭐ 重要消息
          </div>

          <!-- 卡片內容 -->
          <div class="card-content">
            <div class="card-header">
              <h3 class="news-title">{{ news.title }}</h3>
              <div class="status-badges">
                <span v-if="news.isPublished" class="status-badge published">已發布</span>
                <span v-else class="status-badge draft">草稿</span>
              </div>
            </div>

            <p class="news-excerpt">{{ news.excerpt || '無摘要' }}</p>

            <div class="card-meta">
              <div class="meta-info">
                <span class="view-count">👁️ {{ news.viewCount }}</span>
                <span class="publish-date">
                  {{ news.publishedAt ? formatDate(news.publishedAt) : '未發布' }}
                </span>
              </div>

              <div class="card-actions">
                <router-link
                    :to="`/dashboard/news/edit/${news.id}`"
                    class="action-btn edit"
                >
                  編輯
                </router-link>
                <button
                    @click="togglePublish(news)"
                    class="action-btn toggle"
                    :disabled="news.updating"
                >
                  {{ news.isPublished ? '取消發布' : '發布' }}
                </button>
                <button
                    @click="toggleImportant(news)"
                    class="action-btn important"
                    :class="{ active: news.isImportant }"
                    :disabled="news.updating"
                    title="設為重要消息"
                >
                  ⭐
                </button>
                <button
                    @click="deleteNews(news)"
                    class="action-btn delete"
                    :disabled="news.deleting"
                >
                  🗑️
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 分頁 -->
      <div v-if="totalPages > 1" class="pagination">
        <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="page-btn"
        >
          上一頁
        </button>

        <div class="page-numbers">
          <button
              v-for="page in visiblePages"
              :key="page"
              @click="goToPage(page)"
              :class="{ active: page === currentPage }"
              class="page-number"
          >
            {{ page }}
          </button>
        </div>

        <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="page-btn"
        >
          下一頁
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { apiGet, apiPost, apiDelete } from '../utils/api.js'

const loading = ref(true)
const newsList = ref([])
const searchQuery = ref('')
const statusFilter = ref('all')
const currentPage = ref(1)
const pageSize = 12

onMounted(async () => {
  await loadNews()
})

const loadNews = async () => {
  try {
    loading.value = true
    // 這裡需要實作後端 API
    // newsList.value = await apiGet('/api/News')

    // 暫時使用模擬資料
    newsList.value = [
      {
        id: '1',
        title: '2024年度獎學金申請開始！',
        excerpt: '多項獎學金開放申請，包含政府獎學金、學校獎學金等，申請截止日期為2024年12月31日。',
        isPublished: true,
        isImportant: true,
        viewCount: 2340,
        publishedAt: new Date().toISOString(),
        updating: false,
        deleting: false
      },
      {
        id: '2',
        title: '留學說明會活動通知',
        excerpt: '本月將舉辦多場留學說明會，涵蓋美國、英國、澳洲等熱門國家，歡迎有興趣的同學報名參加。',
        isPublished: true,
        isImportant: false,
        viewCount: 856,
        publishedAt: new Date(Date.now() - 86400000).toISOString(),
        updating: false,
        deleting: false
      },
      {
        id: '3',
        title: '新增線上諮詢服務',
        excerpt: '為了提供更便利的服務，我們新增了線上一對一諮詢功能，可以透過視訊方式進行諮詢。',
        isPublished: false,
        isImportant: false,
        viewCount: 0,
        publishedAt: null,
        updating: false,
        deleting: false
      },
      {
        id: '4',
        title: '暑期遊學團開始報名',
        excerpt: '2024年暑期遊學團現正開放報名，包含美國加州、英國倫敦等精彩行程。',
        isPublished: true,
        isImportant: false,
        viewCount: 423,
        publishedAt: new Date(Date.now() - 172800000).toISOString(),
        updating: false,
        deleting: false
      }
    ]
  } catch (error) {
    console.error('Error loading news:', error)
  } finally {
    loading.value = false
  }
}

const filteredNews = computed(() => {
  let filtered = newsList.value

  // 狀態篩選
  if (statusFilter.value === 'published') {
    filtered = filtered.filter(news => news.isPublished)
  } else if (statusFilter.value === 'draft') {
    filtered = filtered.filter(news => !news.isPublished)
  } else if (statusFilter.value === 'important') {
    filtered = filtered.filter(news => news.isImportant)
  }

  // 搜尋篩選
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(news =>
        news.title.toLowerCase().includes(query) ||
        (news.excerpt && news.excerpt.toLowerCase().includes(query))
    )
  }

  // 按重要性和時間排序
  return filtered.sort((a, b) => {
    if (a.isImportant && !b.isImportant) return -1
    if (!a.isImportant && b.isImportant) return 1
    return new Date(b.publishedAt || b.createdAt || 0) - new Date(a.publishedAt || a.createdAt || 0)
  })
})

const totalPages = computed(() =>
    Math.ceil(filteredNews.value.length / pageSize)
)

const paginatedNews = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return filteredNews.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value

  // 顯示當前頁面前後2頁
  for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) {
    pages.push(i)
  }

  return pages
})

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}

const refreshData = async () => {
  await loadNews()
}

const togglePublish = async (news) => {
  try {
    news.updating = true
    // await apiPost(`/api/News/${news.id}/toggle-publish`)
    news.isPublished = !news.isPublished
    if (news.isPublished) {
      news.publishedAt = new Date().toISOString()
    }
    console.log(`${news.isPublished ? '發布' : '取消發布'}消息:`, news.title)
  } catch (error) {
    console.error('Toggle publish failed:', error)
    alert('操作失敗，請稍後再試')
  } finally {
    news.updating = false
  }
}

const toggleImportant = async (news) => {
  try {
    news.updating = true
    // await apiPost(`/api/News/${news.id}/toggle-important`)
    news.isImportant = !news.isImportant
    console.log(`${news.isImportant ? '設為' : '取消'}重要消息:`, news.title)
  } catch (error) {
    console.error('Toggle important failed:', error)
    alert('操作失敗，請稍後再試')
  } finally {
    news.updating = false
  }
}

const deleteNews = async (news) => {
  const confirmed = confirm(`確定要刪除消息「${news.title}」嗎？此操作無法復原。`)
  if (!confirmed) return

  try {
    news.deleting = true
    // await apiDelete(`/api/News/${news.id}`)
    newsList.value = newsList.value.filter(n => n.id !== news.id)
    console.log('消息已刪除:', news.title)
  } catch (error) {
    console.error('Delete failed:', error)
    alert('刪除失敗，請稍後再試')
  } finally {
    news.deleting = false
  }
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

// 當搜尋條件改變時重置到第一頁
computed(() => {
  currentPage.value = 1
  return [searchQuery.value, statusFilter.value]
})
</script>

<style scoped>
.news-list {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
}

.header-left h2 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 700;
  color: #1a202c;
}

.header-left p {
  margin: 0;
  color: #718096;
  font-size: 16px;
}

.create-btn {
  background: #38a169;
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.create-btn:hover {
  background: #2f855a;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(56, 161, 105, 0.3);
}

.filters {
  display: flex;
  gap: 16px;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.search-box {
  flex: 1;
  max-width: 350px;
}

.search-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  transition: all 0.2s ease;
}

.search-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.filter-buttons {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #4a5568;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.filter-btn:hover {
  border-color: #cbd5e0;
  background: #f7fafc;
}

.filter-btn.active {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

.filter-btn.important.active {
  background: #f6ad55;
  border-color: #f6ad55;
}

.refresh-btn {
  background: white;
  border: 1px solid #e2e8f0;
  color: #4a5568;
  padding: 10px 16px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.refresh-btn:hover:not(:disabled) {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.news-container {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.loading {
  text-align: center;
  padding: 80px 20px;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #e2e8f0;
  border-top: 3px solid #3182ce;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.loading p {
  margin: 0;
  color: #718096;
  font-size: 16px;
}

.empty-state {
  text-align: center;
  padding: 80px 20px;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 16px;
  opacity: 0.6;
}

.empty-state p {
  margin: 0;
  color: #718096;
  font-size: 16px;
}

.empty-state a {
  color: #3182ce;
  text-decoration: none;
  font-weight: 500;
}

.empty-state a:hover {
  text-decoration: underline;
}

.news-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 24px;
  padding: 24px;
}

.news-card {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.3s ease;
  background: white;
  position: relative;
}

.news-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.news-card.important {
  border-left: 4px solid #f6ad55;
  background: linear-gradient(135deg, #fffbf0 0%, #ffffff 100%);
}

.important-badge {
  background: #f6ad55;
  color: white;
  padding: 8px 12px;
  font-size: 12px;
  font-weight: 600;
  text-align: center;
}

.card-content {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
  gap: 12px;
}

.news-title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2d3748;
  line-height: 1.4;
  flex: 1;
}

.status-badges {
  flex-shrink: 0;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 12px;
  font-weight: 500;
  text-align: center;
}

.status-badge.published {
  background: #c6f6d5;
  color: #22543d;
}

.status-badge.draft {
  background: #fed7cc;
  color: #c53030;
}

.news-excerpt {
  margin: 0 0 16px 0;
  color: #718096;
  font-size: 14px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding-top: 16px;
  border-top: 1px solid #f7fafc;
}

.meta-info {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #718096;
}

.view-count {
  font-weight: 600;
}

.card-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  padding: 6px 10px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid;
  transition: all 0.2s ease;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 28px;
}

.action-btn.edit {
  background: #edf2f7;
  color: #4a5568;
  border-color: #e2e8f0;
}

.action-btn.edit:hover {
  background: #e2e8f0;
}

.action-btn.toggle {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

.action-btn.toggle:hover:not(:disabled) {
  background: #2c5aa0;
}

.action-btn.important {
  background: white;
  color: #f6ad55;
  border-color: #f6ad55;
}

.action-btn.important:hover:not(:disabled) {
  background: #f6ad55;
  color: white;
}

.action-btn.important.active {
  background: #f6ad55;
  color: white;
}

.action-btn.delete {
  background: white;
  color: #e53e3e;
  border-color: #e53e3e;
}

.action-btn.delete:hover:not(:disabled) {
  background: #e53e3e;
  color: white;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  padding: 24px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.page-btn {
  background: white;
  border: 1px solid #e2e8f0;
  color: #4a5568;
  padding: 10px 16px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.page-btn:hover:not(:disabled) {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-numbers {
  display: flex;
  gap: 4px;
}

.page-number {
  background: white;
  border: 1px solid #e2e8f0;
  color: #4a5568;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
  min-width: 40px;
  text-align: center;
}

.page-number:hover {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.page-number.active {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

/* 響應式設計 */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }

  .filters {
    flex-direction: column;
    align-items: stretch;
  }

  .search-box {
    max-width: none;
  }

  .filter-buttons {
    justify-content: center;
  }

  .news-cards {
    grid-template-columns: 1fr;
    padding: 16px;
    gap: 16px;
  }

  .card-meta {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .card-actions {
    width: 100%;
    justify-content: space-between;
  }

  .page-numbers {
    flex-wrap: wrap;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .news-cards {
    padding: 12px;
  }

  .card-content {
    padding: 16px;
  }

  .pagination {
    flex-direction: column;
    gap: 12px;
  }
}
</style>