import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AppUser } from '~/types/user'
import type { KeyPerson, KeyPersonMyStatus, DesignateKeyPersonRequest } from '~/types/keyPerson'

export const useKeyPersonStore = defineStore('keyPerson', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const keyPersons = ref<KeyPerson[]>([])
  const myStatus = ref<KeyPersonMyStatus | null>(null)
  const allUsers = ref<AppUser[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchList(requesterId: string) {
    loading.value = true
    error.value = null
    try {
      keyPersons.value = await $fetch<KeyPerson[]>(
        `${base}/api/key-persons?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '重点管理人物の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function fetchAllUsers() {
    try {
      allUsers.value = await $fetch<AppUser[]>(`${base}/api/users`)
    }
    catch {
      allUsers.value = []
    }
  }

  async function designate(requesterId: string, req: DesignateKeyPersonRequest) {
    const data = await $fetch<KeyPerson>(
      `${base}/api/key-persons?requesterId=${requesterId}`,
      { method: 'POST', body: req },
    )
    keyPersons.value.push(data)
    return data
  }

  async function revoke(requesterId: string, keyPersonId: string) {
    await $fetch(
      `${base}/api/key-persons/${keyPersonId}?requesterId=${requesterId}`,
      { method: 'DELETE' },
    )
    keyPersons.value = keyPersons.value.filter(kp => kp.id !== keyPersonId)
  }

  async function fetchMyStatus(talentId: string) {
    try {
      myStatus.value = await $fetch<KeyPersonMyStatus>(
        `${base}/api/key-persons/my?talentId=${talentId}`,
      )
    }
    catch {
      myStatus.value = null
    }
  }

  function $reset() {
    keyPersons.value = []
    myStatus.value = null
    allUsers.value = []
    loading.value = false
    error.value = null
  }

  return {
    keyPersons, myStatus, allUsers, loading, error,
    fetchList, fetchAllUsers, designate, revoke, fetchMyStatus, $reset,
  }
})
