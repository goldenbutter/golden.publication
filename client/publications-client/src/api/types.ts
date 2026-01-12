export type PublicationListItem = {
  id: string;
  publication_type: string;
  title: string;
  isbn: string;
  description: string;
};

export type PublicationVersion = {
  id: string;
  publication_guid: string;
  version: string;
  language: string;
  cover_title: string;
};

export type PublicationDetails = {
  id: string;
  publication_type: string;
  title: string;
  isbn: string;
  description: string;
  versions: PublicationVersion[];
};

export type PublicationsListResponse = {
  total: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  items: PublicationListItem[];
};
