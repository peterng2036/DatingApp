import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterState } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;

  member: Member = {} as Member;
  messages: Message[] = [];
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  activeTab?: TabDirective;

  constructor(
    private memberService: MembersService,
    private router: ActivatedRoute,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.router.data.subscribe({
      next: (data) => {
        this.member = data['member'];
      },
    });

    this.router.queryParams.subscribe({
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']);
      },
    });

    this.loadGallery();
  }

  getImages() {
    if (!this.member) return [];

    const imgeUrls = [];

    for (const photo of this.member.photos) {
      imgeUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url,
      });
    }

    return imgeUrls;
  }

  loadGallery() {
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];

    this.galleryImages = this.getImages();
  }

  loadMessage() {
    if (this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: (res) => (this.messages = res),
      });
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages') {
      this.loadMessage();
    }
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const targetTab = this.memberTabs.tabs.find((x) => x.heading === heading);
      if (targetTab) targetTab.active = true;
    }
  }
}
