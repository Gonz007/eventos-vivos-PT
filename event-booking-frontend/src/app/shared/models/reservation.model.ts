export interface Reservation {
  id: number;
  eventId: number;
  eventTitle: string;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
  status: string;
  reservationCode: string | null;
  createdAt: string;
  cancelledAt: string | null;
  isLostSale: boolean;
}

export interface CreateReservationPayload {
  eventId: number;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
}
