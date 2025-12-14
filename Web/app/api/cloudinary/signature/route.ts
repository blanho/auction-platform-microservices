import { NextRequest, NextResponse } from 'next/server';
import crypto from 'crypto';

const CLOUDINARY_API_KEY = process.env.CLOUDINARY_API_KEY;
const CLOUDINARY_API_SECRET = process.env.CLOUDINARY_API_SECRET;
const CLOUDINARY_CLOUD_NAME = process.env.NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME || 'dg6qyxc0a';

export async function POST(request: NextRequest) {
  if (!CLOUDINARY_API_KEY || !CLOUDINARY_API_SECRET) {
    return NextResponse.json(
      { error: 'Cloudinary credentials not configured' },
      { status: 500 }
    );
  }

  const body = await request.json().catch(() => ({}));
  const folder = body.folder || 'auction';

  const timestamp = Math.round(Date.now() / 1000);

  const paramsToSign = `folder=${folder}&timestamp=${timestamp}`;

  const signature = crypto
    .createHash('sha1')
    .update(paramsToSign + CLOUDINARY_API_SECRET)
    .digest('hex');

  return NextResponse.json({
    signature,
    timestamp,
    apiKey: CLOUDINARY_API_KEY,
    cloudName: CLOUDINARY_CLOUD_NAME,
    folder,
  });
}
