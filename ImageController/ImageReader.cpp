#include "pch.h"

/*!
 * @file	ImageReader.cpp
 * @author	kleon6436
 *
 */

#include "RawImageController.h"
#include "NormalImageController.h"
#include "ImageReader.h"
#include <locale.h>
#include <iostream>
#include <windows.h>

namespace Kchary::ImageController::Library
{
	using namespace Kchary::ImageController::RawImageControl;
	using namespace Kchary::ImageController::NormalImageControl;

	ImageReader::ImageReader()
	{
		m_rawImageController = std::make_unique<RawImageController>();
		m_normalImageController = std::make_unique<NormalImageController>();
	}

	bool ImageReader::LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		const auto path = ConvertWcharToString(imagePath);
		m_imageReadSettings = imageReadSettings;

		bool result;
		if (imageReadSettings.isRawImage)
		{
			result = m_rawImageController->LoadImageAndGetImageSize(path.c_str(), imageReadSettings, imageSize);
		}
		else
		{
			result = m_normalImageController->LoadImageAndGetImageSize(path.c_str(), imageReadSettings, imageSize);
		}

		return result;
	}

	bool ImageReader::GetImageData(ImageData& imageData)
	{
		bool result;
		if (m_imageReadSettings.isRawImage)
		{
			result = m_rawImageController->GetImageData(imageData);
		}
		else
		{
			result = m_normalImageController->GetImageData(imageData);
		}

		return result;
	}

	bool ImageReader::GetThumbnailImageData(ImageData& imageData)
	{
		if (!m_imageReadSettings.isThumbnailMode)
		{
			// サムネイルモードで画像を読み込んでいないのでエラーとする
			return false;
		}

		if (m_imageReadSettings.isRawImage)
		{
			m_rawImageController->GetThumbnailImageData(imageData);
		}
		else
		{
			m_normalImageController->GetThumbnailImageData(imageData);
		}

		return true;
	}

	std::string ImageReader::ConvertWcharToString(const wchar_t* imagePath)
	{
		setlocale(LC_CTYPE, "ja_JP.UTF-8");

		std::wstring wide(imagePath);

		// wstring → SJIS
		int iBufferSize = WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, (char*)NULL, 0, NULL, NULL);

		// バッファの取得
		auto* cpMultiByte = new CHAR[iBufferSize];

		// wstring → SJIS
		WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, cpMultiByte, iBufferSize, NULL, NULL);

		// stringの生成
		std::string imagePathStr(cpMultiByte, cpMultiByte + iBufferSize - 1);

		// バッファの破棄
		delete[] cpMultiByte;

		// 変換結果を返す
		return imagePathStr;
	}
}