/**
 * @file	NormalImageController.cpp
 * @author	kleon6436
 */

#include "pch.h"
#include "NormalImageController.h"
#include <fstream>              // std::ifstream
#include <vector>               // std::vector
#include <string>               // std::wstring, std::string
#include <cstring>              // std::memcpy
#include <algorithm>            // std::max

namespace Kchary::ImageController::NormalImageControl
{
    bool NormalImageController::GetImageData(const wchar_t* path, const ImageReadSettings& imageReadSettings, ImageData& imageData)
    {
#ifdef _WIN32
        std::ifstream file(path, std::ios::binary);
#else
        std::ifstream file(std::wstring_convert<std::codecvt_utf8<wchar_t>>{}.to_bytes(path), std::ios::binary);
#endif
        if (!file.is_open())
        {
            return false;
        }

        file.seekg(0, std::ios::end);
        const std::streamsize fileSize = file.tellg();
        if (fileSize <= 0)
        {
            return false;
        }
        file.seekg(0, std::ios::beg);

        std::vector<unsigned char> buffer(static_cast<size_t>(fileSize));
        if (!file.read(reinterpret_cast<char*>(buffer.data()), fileSize))
        {
            return false;
        }

        const int imreadMode = imageReadSettings.isThumbnailMode
            ? GetImreadMode(imageReadSettings.resizeLongSideLength)
            : cv::IMREAD_COLOR;

        cv::Mat image = cv::imdecode(buffer, imreadMode);
        if (image.empty())
        {
            return false;
        }

        if (imageReadSettings.isThumbnailMode)
        {
            const int longSide = std::max(image.cols, image.rows);
            const double ratio = static_cast<double>(imageReadSettings.resizeLongSideLength) / longSide;

            if (ratio < 1.0)
            {
                cv::Mat resized;
                cv::resize(image, resized, cv::Size(), ratio, ratio, cv::INTER_AREA);
                std::swap(image, resized);
            }
        }

        const size_t dataSize = image.total() * image.elemSize();
        imageData.buffer.resize(dataSize); // バッファ確保
        m_image = cv::Mat(image.rows, image.cols, image.type(), imageData.buffer.data(), image.step);
        // 元画像データをコピー（Mat間コピーだと内部最適化が効く）
        image.copyTo(m_image);

        imageData.size = static_cast<unsigned int>(dataSize);
        imageData.stride = static_cast<int>(m_image.step);
        imageData.width = m_image.cols;
        imageData.height = m_image.rows;

        return true;
    }

	cv::ImreadModes NormalImageController::GetImreadMode(const int resizeLongSideLength)
	{
		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= 800)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
		}
		else
		{
			imreadMode = cv::ImreadModes::IMREAD_COLOR;
		}

		return imreadMode;
	}
}