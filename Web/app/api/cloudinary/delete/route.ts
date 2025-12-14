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

  const body = await request.json();
  const { publicId } = body;

  if (!publicId) {
    return NextResponse.json(
      { error: 'Public ID is required' },
      { status: 400 }
    );
  }

  const timestamp = Math.round(Date.now() / 1000);
  const paramsToSign = `public_id=${publicId}&timestamp=${timestamp}`;

  const signature = crypto
    .createHash('sha256')
    .update(paramsToSign + CLOUDINARY_API_SECRET)
    .digest('hex');

  const formData = new FormData();
  formData.append('public_id', publicId);
  formData.append('signature', signature);
  formData.append('api_key', CLOUDINARY_API_KEY);
  formData.append('timestamp', timestamp.toString());

  const response = await fetch(
    `https://api.cloudinary.com/v1_1/${CLOUDINARY_CLOUD_NAME}/image/destroy`,
    {
      method: 'POST',
      body: formData,
    }
  );

  if (!response.ok) {
    return NextResponse.json(
      { error: 'Failed to delete image' },
      { status: 500 }
    );
  }

  return NextResponse.json({ success: true });
}
