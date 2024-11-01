import { Photo } from "./photo"

export interface Member {
  id: number
  userName: string
  knownAs: string
  age: number
  gender: string
  introduction: string
  intrests: string
  country: string
  city: string
  photoUrl: string
  created: Date
  lastActive: Date
  photos: Photo[]
}